#nullable enable
using System;

namespace Mochineko.RelentStateMachine
{
    public interface ITransitionMapBuilder<in TEvent, TContext> : IDisposable
    {
        void RegisterTransition<TFromState, TToState>(TEvent @event)
            where TFromState : IState<TContext>, new()
            where TToState : IState<TContext>, new();
        
        void RegisterAnyTransition<TToState>(TEvent @event)
            where TToState : IState<TContext>, new();

        ITransitionMap<TEvent, TContext> Build();
    }
}