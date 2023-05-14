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

        public static async UniTask<FiniteStateMachine<TEvent, TContext>> CreateAsync(
            ITransitionMap<TEvent, TContext> transitionMap,
            TContext context,
            CancellationToken cancellationToken,
            TimeSpan? semaphoreTimeout = null)
        {
            var instance = new FiniteStateMachine<TEvent, TContext>(
                transitionMap,
                context,
                semaphoreTimeout);

            var enterResult = await instance.currentState
                .EnterAsync(context, cancellationToken);
            switch (enterResult)
            {
                case NoEventRequest<TEvent>:
                    return instance;;

                case ISomeEventRequest<TEvent> eventRequest:
                    var sendEventResult = await instance
                        .SendEventAsync(eventRequest.Event, cancellationToken);
                    return instance;;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(enterResult));
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
                    return Results.Fail(
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
                    return Results.Succeed();

                case IFailureResult<IEventRequest<TEvent>> failureResult:
                    return Results.Fail(
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
                return StateResults.Fail<TEvent>(
                    $"Cancelled to wait semaphore because of {exception}.");
            }

            try
            {
                // Exit current state.
                await currentState.ExitAsync(Context, cancellationToken);
                
                // Enter next state.
                var enterResult = await nextState.EnterAsync(Context, cancellationToken);
                currentState = nextState;
                return Results.Succeed(enterResult);
            }
            catch (OperationCanceledException exception)
            {
                return StateResults.Fail<TEvent>(
                    $"Cancelled to transit state because of {exception}.");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async UniTask UpdateAsync(CancellationToken cancellationToken)
        {
            var updateResult = await currentState.UpdateAsync(Context, cancellationToken);
            switch (updateResult)
            {
                case NoEventRequest<TEvent>:
                    break;
                
                case ISomeEventRequest<TEvent> eventRequest:
                    await SendEventAsync(eventRequest.Event, cancellationToken);
                    return;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(updateResult));
            }
        }
    }
}