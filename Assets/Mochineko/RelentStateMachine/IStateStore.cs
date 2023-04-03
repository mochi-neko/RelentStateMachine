#nullable enable
using System;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public interface IStateStore<in TEvent, TContext> : IDisposable
    {
        void AddTransition<TFromState, TToState>(TEvent @event)
            where TFromState : IState<TContext>, new()
            where TToState : IState<TContext>, new();
        
        IState<TContext> InitialState { get; }
        IResult<IState<TContext>> CanTransit(IState<TContext> currentState, TEvent @event);
    }
}