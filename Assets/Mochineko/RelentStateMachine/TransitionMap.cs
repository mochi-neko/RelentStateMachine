#nullable enable
using System.Collections.Generic;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    internal sealed class TransitionMap<TEvent, TContext> : ITransitionMap<TEvent, TContext>
    {
        private readonly IState<TContext> initialState;
        private readonly IReadOnlyList<IState<TContext>> states;

        private readonly IReadOnlyDictionary<
                IState<TContext>,
                IReadOnlyDictionary<TEvent, IState<TContext>>>
            transitionMap;

        private readonly IReadOnlyDictionary<TEvent, IState<TContext>>
            anyTransitionMap;

        public TransitionMap(
            IState<TContext> initialState,
            IReadOnlyList<IState<TContext>> states,
            IReadOnlyDictionary<IState<TContext>, IReadOnlyDictionary<TEvent, IState<TContext>>> transitionMap,
            IReadOnlyDictionary<TEvent, IState<TContext>> anyTransitionMap)
        {
            this.initialState = initialState;
            this.states = states;
            this.transitionMap = transitionMap;
            this.anyTransitionMap = anyTransitionMap;
        }

        IState<TContext> ITransitionMap<TEvent, TContext>.InitialState
            => initialState;

        IResult<IState<TContext>> ITransitionMap<TEvent, TContext>.CanTransit(
            IState<TContext> currentState,
            TEvent @event)
        {
            if (transitionMap.TryGetValue(currentState, out var candidates))
            {
                if (candidates.TryGetValue(@event, out var nextState))
                {
                    return ResultFactory.Succeed(nextState);
                }
            }
            
            if (anyTransitionMap.TryGetValue(@event, out var nextStateFromAny))
            {
                return ResultFactory.Succeed(nextStateFromAny);
            }

            return ResultFactory.Fail<IState<TContext>>(
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