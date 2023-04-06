#nullable enable
using System;
using System.Collections.Generic;

namespace Mochineko.RelentStateMachine
{
    public sealed class StateStore<TContext> : IStateStore<TContext>
    {
        private readonly IStackState<TContext> initialState;
        private readonly IReadOnlyList<IStackState<TContext>> states;

        public StateStore(
            IStackState<TContext> initialState,
            IReadOnlyList<IStackState<TContext>> states)
        {
            this.initialState = initialState;
            this.states = states;
        }

        IStackState<TContext> IStateStore<TContext>.InitialState
            => initialState;

        IStackState<TContext> IStateStore<TContext>.Get<TState>()
        {
            foreach (var state in states)
            {
                if (state is TState target)
                {
                    return target;
                }
            }

            throw new ArgumentException($"Not found state: {typeof(TState)}");
        }

        public void Dispose()
        {
            foreach (var state in states)
            {
                state.Dispose();
            }
        }
    }
}