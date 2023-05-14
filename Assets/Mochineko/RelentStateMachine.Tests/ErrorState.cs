#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class ErrorState : IState<MockEvent, MockContext>
    {
        public async UniTask<IEventRequest<MockEvent>> EnterAsync(MockContext context,
            CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);

            context.ErrorMessage = "Manual Error";

            return EventRequests.None<MockEvent>();
        }

        public async UniTask<IEventRequest<MockEvent>> UpdateAsync(MockContext context,
            CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);

            return EventRequests.None<MockEvent>();
        }

        public async UniTask ExitAsync(MockContext context, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}