#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public interface IFiniteStateMachine<TEvent, out TContext> : IDisposable
    {
        TContext Context { get; }
        bool IsCurrentState<TState>() where TState : IState<TEvent, TContext>;
        UniTask<IResult> SendEventAsync(TEvent @event, CancellationToken cancellationToken);
        UniTask UpdateAsync(CancellationToken cancellationToken);
    }
}