#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public sealed class StackStateMachine<TContext>
        : IStackStateMachine<TContext>
    {
        private readonly IStateStore<TContext> stateStore;
        public TContext Context { get; }
        private readonly Stack<IStackState<TContext>> stack = new();

        public bool IsCurrentState<TState>()
            where TState : IStackState<TContext>
            => stack.Peek() is TState;

        private readonly SemaphoreSlim semaphore = new(
            initialCount: 1,
            maxCount: 1);

        private readonly TimeSpan semaphoreTimeout;
        private const float DefaultSemaphoreTimeoutSeconds = 30f;

        public static async UniTask<IResult<StackStateMachine<TContext>>> CreateAsync(
            IStateStore<TContext> stateStore,
            TContext context,
            CancellationToken cancellationToken,
            TimeSpan? semaphoreTimeout = null)
        {
            var instance = new StackStateMachine<TContext>(
                stateStore,
                context,
                semaphoreTimeout);

            var initializeResult = await instance.stack.Peek()
                .EnterAsync(context, cancellationToken);
            switch (initializeResult)
            {
                case ISuccessResult:
                    return Results.Succeed(instance);

                case IFailureResult initializeFailure:
                    return Results.Fail<StackStateMachine<TContext>>(
                        $"Failed to enter initial state because of {initializeFailure.Message}.");

                default:
                    throw new ResultPatternMatchException(nameof(initializeResult));
            }
        }

        private StackStateMachine(
            IStateStore<TContext> stateStore,
            TContext context,
            TimeSpan? semaphoreTimeout = null)
        {
            this.stateStore = stateStore;
            this.Context = context;
            this.stack.Push(this.stateStore.InitialState);

            this.semaphoreTimeout =
                semaphoreTimeout
                ?? TimeSpan.FromSeconds(DefaultSemaphoreTimeoutSeconds);
        }

        public void Dispose()
        {
            semaphore.Dispose();
            stack.Clear();
            stateStore.Dispose();
        }

        public async UniTask<IResult<IPopToken>> PushAsync<TState>(
            CancellationToken cancellationToken)
            where TState : IStackState<TContext>
        {
            // Make stack thread-safe.
            try
            {
                await semaphore.WaitAsync(semaphoreTimeout, cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                semaphore.Release();
                return Results.Fail<IPopToken>(
                    $"Cancelled to wait semaphore because of {exception}.");
            }

            var nextState = stateStore.Get<TState>();
            try
            {
                var enterResult = await nextState.EnterAsync(Context, cancellationToken);
                switch (enterResult)
                {
                    case ISuccessResult:
                        stack.Push(nextState);
                        return Results.Succeed(PopToken.Publish(this));

                    case IFailureResult enterFailure:
                        return Results.Fail<IPopToken>(
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
            var currentState = stack.Peek();
            var updateResult = await currentState.UpdateAsync(Context, cancellationToken);
            switch (updateResult)
            {
                case ISuccessResult:
                    return Results.Succeed();

                case IFailureResult updateFailure:
                    return Results.Fail(
                        $"Failed to update current state:{currentState.GetType()} because of {updateFailure.Message}.");

                default:
                    throw new ResultPatternMatchException(nameof(updateResult));
            }
        }

        private sealed class PopToken : IPopToken
        {
            private readonly StackStateMachine<TContext> publisher;
            private bool popped = false;

            public static IPopToken Publish(StackStateMachine<TContext> publisher)
                => new PopToken(publisher);

            private PopToken(StackStateMachine<TContext> publisher)
            {
                this.publisher = publisher;
            }

            public async UniTask<IResult> PopAsync(CancellationToken cancellationToken)
            {
                if (popped)
                {
                    throw new InvalidOperationException(
                        $"Failed to pop because of already popped.");
                }
                
                if (publisher.stack.Count is 0)
                {
                    throw new InvalidOperationException(
                        $"Failed to pop because of stack is empty.");
                }

                // Make stack thread-safe.
                try
                {
                    await publisher.semaphore
                        .WaitAsync(publisher.semaphoreTimeout, cancellationToken);
                }
                catch (OperationCanceledException exception)
                {
                    publisher.semaphore.Release();
                    return Results.Fail(
                        $"Cancelled to wait semaphore because of {exception}.");
                }

                popped = true;
                
                var currentState = publisher.stack.Peek();
                var exitResult = await currentState
                    .ExitAsync(publisher.Context, cancellationToken);
                try
                {
                    switch (exitResult)
                    {
                        case ISuccessResult:
                            _ = publisher.stack.Pop();
                            return Results.Succeed();

                        case IFailureResult updateFailure:
                            return Results.Fail(
                                $"Failed to exit current state:{currentState.GetType()} because of {updateFailure.Message}.");

                        default:
                            throw new ResultPatternMatchException(nameof(exitResult));
                    }
                }
                finally
                {
                    publisher.semaphore.Release();
                }
            }
        }
    }
}