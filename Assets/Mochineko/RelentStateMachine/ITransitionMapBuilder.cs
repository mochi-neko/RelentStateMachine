#nullable enable
using System;

namespace Mochineko.RelentStateMachine
{
    public interface ITransitionMapBuilder<TEvent, TContext> : IDisposable
    {
        void RegisterTransition<TFromState, TToState>(TEvent @event)
            where TFromState : IState<TEvent, TContext>, new()
            where TToState : IState<TEvent, TContext>, new();

        void RegisterAnyTransition<TToState>(TEvent @event)
            where TToState : IState<TEvent, TContext>, new();

        ITransitionMap<TEvent, TContext> Build();
    }
}