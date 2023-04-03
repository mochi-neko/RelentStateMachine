#nullable enable
using System;
using System.Collections.Generic;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public sealed class StateStore<TEvent, TContext> : IStateStore<TEvent, TContext>
    {
        private readonly IState<TContext> initialState;
        private readonly List<IState<TContext>> states = new();

        private readonly Dictionary<IState<TContext>, Dictionary<TEvent, IState<TContext>>>
            transitionMap = new();

        IState<TContext> IStateStore<TEvent, TContext>.InitialState
            => initialState;

        public static StateStore<TEvent, TContext> Create<TInitialState>()
            where TInitialState : IState<TContext>, new()
        {
            var initialState = new TInitialState();
            return new StateStore<TEvent, TContext>(initialState);
        }

        private StateStore(IState<TContext> initialState)
        {
            this.initialState = initialState;
            states.Add(this.initialState);
        }

        public void AddTransition<TFromState, TToState>(TEvent @event)
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

        IResult<IState<TContext>> IStateStore<TEvent, TContext>.CanTransit(
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
            transitionMap.Clear();

            foreach (var state in states)
            {
                state.Dispose();
            }

            states.Clear();
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