#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Mochineko.RelentStateMachine
{
    public interface IState<TEvent, in TContext> : IDisposable
    {
        UniTask<IEventRequest<TEvent>> EnterAsync(TContext context, CancellationToken cancellationToken);
        UniTask<IEventRequest<TEvent>> UpdateAsync(TContext context, CancellationToken cancellationToken);
        UniTask ExitAsync(TContext context, CancellationToken cancellationToken);
    }
}