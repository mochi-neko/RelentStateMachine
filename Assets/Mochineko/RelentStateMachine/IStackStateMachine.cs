#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public interface IStackStateMachine<out TContext> : IDisposable
    {
        TContext Context { get; }
        bool IsCurrentState<TState>() where TState : IStackState<TContext>;

        UniTask<IResult<IPopToken>> PushAsync<TState>(CancellationToken cancellationToken)
            where TState : IStackState<TContext>;

        UniTask UpdateAsync(CancellationToken cancellationToken);
    }
}