#nullable enable
using System;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public interface ITransitionMap<in TEvent, TContext> : IDisposable
    {
        internal IState<TContext> InitialState { get; }
        internal IResult<IState<TContext>> CanTransit(IState<TContext> currentState, TEvent @event);
    }
}