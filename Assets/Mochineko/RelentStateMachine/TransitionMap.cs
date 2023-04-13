#nullable enable
using System.Collections.Generic;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    internal sealed class TransitionMap<TEvent, TContext>
        : ITransitionMap<TEvent, TContext>
    {
        private readonly IState<TEvent, TContext> initialState;
        private readonly IReadOnlyList<IState<TEvent, TContext>> states;

        private readonly IReadOnlyDictionary<
                IState<TEvent, TContext>,
                IReadOnlyDictionary<TEvent, IState<TEvent, TContext>>>
            transitionMap;

        private readonly IReadOnlyDictionary<TEvent, IState<TEvent, TContext>>
            anyTransitionMap;

        public TransitionMap(
            IState<TEvent, TContext> initialState,
            IReadOnlyList<IState<TEvent, TContext>> states,
            IReadOnlyDictionary<
                    IState<TEvent, TContext>,
                    IReadOnlyDictionary<TEvent, IState<TEvent, TContext>>>
                transitionMap,
            IReadOnlyDictionary<TEvent, IState<TEvent, TContext>> anyTransitionMap)
        {
            this.initialState = initialState;
            this.states = states;
            this.transitionMap = transitionMap;
            this.anyTransitionMap = anyTransitionMap;
        }

        IState<TEvent, TContext> ITransitionMap<TEvent, TContext>.InitialState
            => initialState;

        IResult<IState<TEvent, TContext>> ITransitionMap<TEvent, TContext>.AllowedToTransit(
            IState<TEvent, TContext> currentState,
            TEvent @event)
        {
            if (transitionMap.TryGetValue(currentState, out var candidates))
            {
                if (candidates.TryGetValue(@event, out var nextState))
                {
                    return Results.Succeed(nextState);
                }
            }

            if (anyTransitionMap.TryGetValue(@event, out var nextStateFromAny))
            {
                return Results.Succeed(nextStateFromAny);
            }

            return Results.Fail<IState<TEvent, TContext>>(
                $"Not found transition from {currentState.GetType()} with event {@event}.");
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