#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public sealed class FiniteStateMachine<TEvent, TContext>
        : IFiniteStateMachine<TEvent, TContext>
    {
        private readonly ITransitionMap<TEvent, TContext> transitionMap;
        public TContext Context { get; }
        private IState<TEvent, TContext> currentState;

        public bool IsCurrentState<TState>()
            where TState : IState<TEvent, TContext>
            => currentState is TState;

        private readonly SemaphoreSlim semaphore = new(
            initialCount: 1,
            maxCount: 1);

        private readonly TimeSpan semaphoreTimeout;
        private const float DefaultSemaphoreTimeoutSeconds = 30f;

        public static async UniTask<IResult<FiniteStateMachine<TEvent, TContext>>> CreateAsync(
            ITransitionMap<TEvent, TContext> transitionMap,
            TContext context,
            CancellationToken cancellationToken,
            TimeSpan? semaphoreTimeout = null)
        {
            var instance = new FiniteStateMachine<TEvent, TContext>(
                transitionMap,
                context,
                semaphoreTimeout);

            var initializeResult = await instance.currentState
                .EnterAsync(context, cancellationToken);
            switch (initializeResult)
            {
                // Chains immediate event sending.
                case ISuccessResult<IEventRequest<TEvent>> initializeSuccess
                    when initializeSuccess.Result is ISomeEventRequest<TEvent> eventRequest:
                {
                    var sendEventResult = await instance
                        .SendEventAsync(eventRequest.Event, cancellationToken);
                    return sendEventResult switch
                    {
                        ISuccessResult
                            => ResultFactory.Succeed(instance),

                        IFailureResult sendEventFailure
                            => ResultFactory.Fail<FiniteStateMachine<TEvent, TContext>>(
                                $"Failed to send event at initialization because of {sendEventFailure.Message}."),

                        _ => throw new ResultPatternMatchException(nameof(sendEventResult))
                    };
                }

                case ISuccessResult<IEventRequest<TEvent>> initializeSuccess:
                    return ResultFactory.Succeed(instance);

                case IFailureResult<IEventRequest<TEvent>> initializeFailure:
                    return ResultFactory.Fail<FiniteStateMachine<TEvent, TContext>>(
                        $"Failed to enter initial state because of {initializeFailure.Message}.");

                default:
                    throw new ResultPatternMatchException(nameof(initializeResult));
            }
        }

        private FiniteStateMachine(
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
        
        public void Dispose()
        {
            transitionMap.Dispose();
            semaphore.Dispose();
        }

        public async UniTask<IResult> SendEventAsync(
            TEvent @event,
            CancellationToken cancellationToken)
        {
            // Check transition.
            IState<TEvent, TContext> nextState;
            var transitionCheckResult = transitionMap.AllowedToTransit(currentState, @event);
            switch (transitionCheckResult)
            {
                case ISuccessResult<IState<TEvent, TContext>> transitionSuccess:
                    nextState = transitionSuccess.Result;
                    break;

                case IFailureResult<IState<TEvent, TContext>> transitionFailure:
                    return ResultFactory.Fail(
                        $"Failed to transit state because of {transitionFailure.Message}.");

                default:
                    throw new ResultPatternMatchException(nameof(transitionCheckResult));
            }

            var transitResult = await TransitAsync(nextState, cancellationToken);
            switch (transitResult)
            {
                case ISuccessResult<IEventRequest<TEvent>> successResult
                    when successResult.Result is ISomeEventRequest<TEvent> eventRequest:
                    // NOTE: Recursive calling.
                    return await SendEventAsync(eventRequest.Event, cancellationToken);

                case ISuccessResult<IEventRequest<TEvent>> successResult:
                    return ResultFactory.Succeed();

                case IFailureResult<IEventRequest<TEvent>> failureResult:
                    return ResultFactory.Fail(
                        $"Failed to transit state from {currentState.GetType()} to {nextState.GetType()} because of {failureResult.Message}.");

                default:
                    throw new ResultPatternMatchException(nameof(transitResult));
            }
        }

        private async UniTask<IResult<IEventRequest<TEvent>>> TransitAsync(
            IState<TEvent, TContext> nextState,
            CancellationToken cancellationToken)
        {
            // Make state thread-safe.
            try
            {
                await semaphore.WaitAsync(semaphoreTimeout, cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                semaphore.Release();
                return StateResultFactory.Fail<TEvent>(
                    $"Cancelled to wait semaphore because of {exception}.");
            }

            try
            {
                // Exit current state.
                var exitResult = await currentState.ExitAsync(Context, cancellationToken);
                if (exitResult is IFailureResult exitFailure)
                {
                    return StateResultFactory.Fail<TEvent>(
                        $"Failed to exit current state:{currentState.GetType()} because of {exitFailure.Message}.");
                }

                // Enter next state.
                var enterResult = await nextState.EnterAsync(Context, cancellationToken);
                switch (enterResult)
                {
                    case ISuccessResult<IEventRequest<TEvent>> enterSuccess:
                        currentState = nextState;
                        return ResultFactory.Succeed(enterSuccess.Result);

                    case IFailureResult<IEventRequest<TEvent>> enterFailure:
                        return StateResultFactory.Fail<TEvent>(
                            $"Failed to enter state:{nextState.GetType()} because of {enterFailure.Message}.");

                    default:
                        throw new ResultPatternMatchException(nameof(enterResult));
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async UniTask<IResult> UpdateAsync(CancellationToken cancellationToken)
        {
            var updateResult = await currentState.UpdateAsync(Context, cancellationToken);
            switch (updateResult)
            {
                case ISuccessResult<IEventRequest<TEvent>> updateSuccess
                    when updateSuccess.Result is ISomeEventRequest<TEvent> eventRequest:
                    // NOTE: Can be recursive calling.
                    return await SendEventAsync(eventRequest.Event, cancellationToken);

                case ISuccessResult<IEventRequest<TEvent>> updateSuccess:
                    return ResultFactory.Succeed();

                case IFailureResult<IEventRequest<TEvent>> updateFailure:
                    return ResultFactory.Fail(
                        $"Failed to update current state:{currentState.GetType()} because of {updateFailure.Message}.");

                default:
                    throw new ResultPatternMatchException(nameof(updateResult));
            }
        }
    }
}