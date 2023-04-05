#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public sealed class StateMachine<TEvent, TContext>
        : IStateMachine<TEvent, TContext>
    {
        private readonly ITransitionMap<TEvent, TContext> transitionMap;
        public TContext Context { get; }
        private IState<TEvent, TContext> currentState;

        public bool IsCurrentState<TState>()
            where TState : IState<TEvent, TContext>
            => currentState is TState;

        private readonly SemaphoreSlim semaphoreSlim = new(
            initialCount: 1,
            maxCount: 1);

        private readonly TimeSpan semaphoreTimeout;
        private const float DefaultSemaphoreTimeoutSeconds = 30f;

        public static async UniTask<IResult<StateMachine<TEvent, TContext>>> CreateAsync(
            ITransitionMap<TEvent, TContext> transitionMap,
            TContext context,
            CancellationToken cancellationToken,
            TimeSpan? semaphoreTimeout = null)
        {
            var instance = new StateMachine<TEvent, TContext>(
                transitionMap,
                context,
                semaphoreTimeout);

            var initializeResult = await instance.currentState
                .EnterAsync(context, cancellationToken);
            if (initializeResult is ISuccessResult<IEventRequest<TEvent>> initializeSuccess)
            {
                // Chains immediate event sending.
                if (initializeSuccess.Result is ISomeEventRequest<TEvent> eventRequest)
                {
                    var sendEventResult = await instance
                        .SendEventAsync(eventRequest.Event, cancellationToken);
                    if (sendEventResult.Success)
                    {
                        return ResultFactory.Succeed(instance);
                    }
                    else if (sendEventResult is IFailureResult sendEventFailure)
                    {
                        return ResultFactory.Fail<StateMachine<TEvent, TContext>>(
                            $"Failed to send event at initialization because of {sendEventFailure.Message}.");
                    }
                    else
                    {
                        throw new ResultPatternMatchException(nameof(sendEventResult));
                    }
                }
                else
                {
                    return ResultFactory.Succeed(instance);
                }
            }
            else if (initializeResult is IFailureResult<IEventRequest<TEvent>> initializeFailure)
            {
                return ResultFactory.Fail<StateMachine<TEvent, TContext>>(
                    $"Failed to enter initial state because of {initializeFailure.Message}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(initializeResult));
            }
        }

        private StateMachine(
            ITransitionMap<TEvent, TContext> transitionMap,
            TContext context,
            TimeSpan? semaphoreTimeout = null)
        {
            this.transitionMap = transitionMap;
            this.Context = context;
            this.currentState = this.transitionMap.InitialState;

            this.semaphoreTimeout =
                semaphoreTimeout
                ?? TimeSpan.FromSeconds(DefaultSemaphoreTimeoutSeconds);
        }

        public async UniTask<IResult> SendEventAsync(
            TEvent @event,
            CancellationToken cancellationToken)
        {
            // Restrict to one event at a time.
            try
            {
                await semaphoreSlim.WaitAsync(semaphoreTimeout, cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                semaphoreSlim.Release();
                return ResultFactory.Fail(
                    $"Cancelled to send event because of {exception}.");
            }

            TEvent continueEvent;
            try
            {
                IState<TEvent, TContext> nextState;
                var transitResult = transitionMap.CanTransit(currentState, @event);
                if (transitResult is ISuccessResult<IState<TEvent, TContext>> transitionSuccess)
                {
                    nextState = transitionSuccess.Result;
                }
                else if (transitResult is IFailureResult<IState<TEvent, TContext>> transitionFailure)
                {
                    return ResultFactory.Fail(
                        $"Failed to transit state because of {transitionFailure.Message}.");
                }
                else
                {
                    throw new ResultPatternMatchException(nameof(transitResult));
                }

                var exitResult = await currentState.ExitAsync(Context, cancellationToken);
                if (exitResult is IFailureResult exitFailure)
                {
                    return ResultFactory.Fail(
                        $"Failed to exit current state:{currentState.GetType()} because of {exitFailure.Message}.");
                }

                var enterResult = await nextState.EnterAsync(Context, cancellationToken);
                if (enterResult is ISuccessResult<IEventRequest<TEvent>> enterSuccess)
                {
                    currentState = nextState;

                    if (enterSuccess.Result is ISomeEventRequest<TEvent> eventRequest)
                    {
                        // Continue to send event.
                        continueEvent = eventRequest.Event;
                    }
                    else
                    {
                        return ResultFactory.Succeed();
                    }
                }
                else if (enterResult is IFailureResult<IEventRequest<TEvent>> enterFailure)
                {
                    return ResultFactory.Fail(
                        $"Failed to enter state:{nextState.GetType()} because of {enterFailure.Message}.");
                }
                else
                {
                    throw new ResultPatternMatchException(nameof(enterResult));
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }

            // NOTE: Recursive calling.
            return await SendEventAsync(continueEvent, cancellationToken);
        }

        public async UniTask<IResult> UpdateAsync(CancellationToken cancellationToken)
        {
            var updateResult = await currentState.UpdateAsync(Context, cancellationToken);
            if (updateResult is ISuccessResult<IEventRequest<TEvent>> updateSuccess)
            {
                if (updateSuccess.Result is ISomeEventRequest<TEvent> eventRequest)
                {
                    // NOTE: Can be recursive calling.
                    return await SendEventAsync(eventRequest.Event, cancellationToken);
                }
                else
                {
                    return ResultFactory.Succeed();
                }
            }
            else if (updateResult is IFailureResult<IEventRequest<TEvent>> updateFailure)
            {
                return ResultFactory.Fail(
                    $"Failed to update current state:{currentState.GetType()} because of {updateFailure.Message}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(updateResult));
            }
        }

        public void Dispose()
        {
            transitionMap.Dispose();
            semaphoreSlim.Dispose();
        }
    }
}