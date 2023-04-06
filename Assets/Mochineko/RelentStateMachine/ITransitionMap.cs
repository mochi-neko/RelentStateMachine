#nullable enable
using System;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public interface ITransitionMap<TEvent, TContext> : IDisposable
    {
        internal IState<TEvent, TContext> InitialState { get; }
        internal IResult<IState<TEvent, TContext>> AllowedToTransit(IState<TEvent, TContext> currentState, TEvent @event);
    }
}