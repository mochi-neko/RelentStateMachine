#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public interface IStackState<in TContext> : IDisposable
    {
        UniTask<IResult> EnterAsync(TContext context, CancellationToken cancellationToken);
        UniTask<IResult> UpdateAsync(TContext context, CancellationToken cancellationToken);
        UniTask<IResult> ExitAsync(TContext context, CancellationToken cancellationToken);
    }
}