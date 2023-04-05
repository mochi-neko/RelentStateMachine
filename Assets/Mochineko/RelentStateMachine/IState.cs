#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public interface IState<TEvent, in TContext> : IDisposable
    {
        UniTask<IResult<IEventRequest<TEvent>>> EnterAsync(TContext context, CancellationToken cancellationToken);
        UniTask<IResult<IEventRequest<TEvent>>> UpdateAsync(TContext context, CancellationToken cancellationToken);
        UniTask<IResult> ExitAsync(TContext context, CancellationToken cancellationToken);
    }
}