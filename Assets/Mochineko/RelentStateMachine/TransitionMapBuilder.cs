#nullable enable
using System;
using System.Collections.Generic;

namespace Mochineko.RelentStateMachine
{
    public sealed class TransitionMapBuilder<TEvent, TContext> : ITransitionMapBuilder<TEvent, TContext>
    {
        private readonly IState<TContext> initialState;
        private readonly List<IState<TContext>> states = new();

        private readonly Dictionary<IState<TContext>, Dictionary<TEvent, IState<TContext>>>
            transitionMap = new();

        public static TransitionMapBuilder<TEvent, TContext> Create<TInitialState>()
            where TInitialState : IState<TContext>, new()
        {
            var initialState = new TInitialState();
            return new TransitionMapBuilder<TEvent, TContext>(initialState);
        }

        private TransitionMapBuilder(IState<TContext> initialState)
        {
            this.initialState = initialState;
            states.Add(this.initialState);
        }

        public void RegisterTransition<TFromState, TToState>(TEvent @event)
            where TFromState : IState<TContext>, new()
            where TToState : IState<TContext>, new()
        {
            var fromState = GetOrCreateState<TFromState>();
            var toState = GetOrCreateState<TToState>();

            if (transitionMap.TryGetValue(fromState, out var transitions))
            {
                if (transitions.TryGetValue(@event, out var nextState))
                {
                    throw new InvalidOperationException(
                        $"Already exists transition from {fromState.GetType()} to {nextState.GetType()} with event {@event}.");
                }
                else
                {
                    transitions.Add(@event, toState);
                }
            }
            else
            {
                var newTransitions = new Dictionary<TEvent, IState<TContext>>();
                newTransitions.Add(@event, toState);
                transitionMap.Add(fromState, newTransitions);
            }
        }

        public ITransitionMap<TEvent, TContext> Build()
            => new TransitionMap<TEvent, TContext>(
                initialState,
                states,
                BuildReadonlyTransitionMap());

        private IReadOnlyDictionary<
                IState<TContext>,
                IReadOnlyDictionary<TEvent, IState<TContext>>>
            BuildReadonlyTransitionMap()
        {
            var result = new Dictionary<
                IState<TContext>,
                IReadOnlyDictionary<TEvent, IState<TContext>>>();
            foreach (var (key, value) in transitionMap)
            {
                result.Add(key, value);
            }

            return result;
        }

        private TState GetOrCreateState<TState>()
            where TState : IState<TContext>, new()
        {
            foreach (var state in states)
            {
                if (state is TState target)
                {
                    return target;
                }
            }

            var newState = new TState();
            states.Add(newState);
            return newState;
        }
    }
}