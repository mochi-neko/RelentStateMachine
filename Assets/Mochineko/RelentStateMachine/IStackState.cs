#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public interface IStackState<in TContext> : IDisposable
    {
        UniTask EnterAsync(TContext context, CancellationToken cancellationToken);
        UniTask UpdateAsync(TContext context, CancellationToken cancellationToken);
        UniTask ExitAsync(TContext context, CancellationToken cancellationToken);
    }
}