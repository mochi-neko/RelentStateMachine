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

        public TransitionMap(
            IState<TContext> initialState,
            IReadOnlyList<IState<TContext>> states,
            IReadOnlyDictionary<IState<TContext>, IReadOnlyDictionary<TEvent, IState<TContext>>> transitionMap)
        {
            this.initialState = initialState;
            this.states = states;
            this.transitionMap = transitionMap;
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
                else
                {
                    return ResultFactory.Fail<IState<TContext>>(
                        $"Not found transition from {currentState.GetType()} with event {@event}.");
                }
            }
            else
            {
                return ResultFactory.Fail<IState<TContext>>(
                    $"Not found transition from {currentState.GetType()}.");
            }
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