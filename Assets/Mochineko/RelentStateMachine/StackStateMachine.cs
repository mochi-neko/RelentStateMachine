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

        public static async UniTask<StackStateMachine<TContext>> CreateAsync(
            IStateStore<TContext> stateStore,
            TContext context,
            CancellationToken cancellationToken,
            TimeSpan? semaphoreTimeout = null)
        {
            var instance = new StackStateMachine<TContext>(
                stateStore,
                context,
                semaphoreTimeout);

            await instance.stack.Peek()
                .EnterAsync(context, cancellationToken);

            return instance;
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
                await nextState.EnterAsync(Context, cancellationToken);

                stack.Push(nextState);
                return Results.Succeed(PopToken.Publish(this));
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async UniTask UpdateAsync(CancellationToken cancellationToken)
        {
            var currentState = stack.Peek();
            await currentState.UpdateAsync(Context, cancellationToken);
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
                try
                {
                    await currentState
                        .ExitAsync(publisher.Context, cancellationToken);

                    publisher.stack.Pop();
                    return Results.Succeed();
                }
                finally
                {
                    publisher.semaphore.Release();
                }
            }
        }
    }
}