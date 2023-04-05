#nullable enable
using System;
using System.Collections.Generic;

namespace Mochineko.RelentStateMachine
{
    public sealed class TransitionMapBuilder<TEvent, TContext>
        : ITransitionMapBuilder<TEvent, TContext>
    {
        private readonly IState<TEvent, TContext> initialState;
        private readonly List<IState<TEvent, TContext>> states = new();

        private readonly Dictionary<IState<TEvent, TContext>, Dictionary<TEvent, IState<TEvent, TContext>>>
            transitionMap = new();

        private readonly Dictionary<TEvent, IState<TEvent, TContext>>
            anyTransitionMap = new();

        private bool disposed = false;

        public static TransitionMapBuilder<TEvent, TContext> Create<TInitialState>()
            where TInitialState : IState<TEvent, TContext>, new()
        {
            var initialState = new TInitialState();
            return new TransitionMapBuilder<TEvent, TContext>(initialState);
        }

        private TransitionMapBuilder(IState<TEvent, TContext> initialState)
        {
            this.initialState = initialState;
            states.Add(this.initialState);
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(TransitionMapBuilder<TEvent, TContext>));
            }

            disposed = true;
        }

        public void RegisterTransition<TFromState, TToState>(TEvent @event)
            where TFromState : IState<TEvent, TContext>, new()
            where TToState : IState<TEvent, TContext>, new()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(TransitionMapBuilder<TEvent, TContext>));
            }

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
                var newTransitions = new Dictionary<TEvent, IState<TEvent, TContext>>();
                newTransitions.Add(@event, toState);
                transitionMap.Add(fromState, newTransitions);
            }
        }

        public void RegisterAnyTransition<TToState>(TEvent @event)
            where TToState : IState<TEvent, TContext>, new()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(TransitionMapBuilder<TEvent, TContext>));
            }

            var toState = GetOrCreateState<TToState>();

            if (anyTransitionMap.TryGetValue(@event, out var nextState))
            {
                throw new InvalidOperationException(
                    $"Already exists transition from any state to {nextState.GetType()} with event {@event}.");
            }
            else
            {
                anyTransitionMap.Add(@event, toState);
            }
        }

        public ITransitionMap<TEvent, TContext> Build()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(TransitionMapBuilder<TEvent, TContext>));
            }

            var result = new TransitionMap<TEvent, TContext>(
                initialState,
                states,
                BuildReadonlyTransitionMap(),
                anyTransitionMap);

            // Cannot reuse builder after build.
            this.Dispose();

            return result;
        }

        private IReadOnlyDictionary<
                IState<TEvent, TContext>,
                IReadOnlyDictionary<TEvent, IState<TEvent, TContext>>>
            BuildReadonlyTransitionMap()
        {
            var result = new Dictionary<
                IState<TEvent, TContext>,
                IReadOnlyDictionary<TEvent, IState<TEvent, TContext>>>();
            foreach (var (key, value) in transitionMap)
            {
                result.Add(key, value);
            }

            return result;
        }

        private TState GetOrCreateState<TState>()
            where TState : IState<TEvent, TContext>, new()
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