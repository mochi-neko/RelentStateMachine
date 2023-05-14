#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class ActiveState : IState<MockEvent, MockContext>
    {
        async UniTask<IEventRequest<MockEvent>> IState<MockEvent, MockContext>.EnterAsync(
            MockContext context,
            CancellationToken cancellationToken)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(0.01f),
                cancellationToken: cancellationToken);

            context.Active = true;

            return EventRequests.None<MockEvent>();
        }

        public async UniTask<IEventRequest<MockEvent>> UpdateAsync(MockContext context,
            CancellationToken cancellationToken)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(0.01f),
                cancellationToken: cancellationToken);

            return EventRequests.None<MockEvent>();
        }

        public async UniTask ExitAsync(MockContext context, CancellationToken cancellationToken)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(0.01f),
                cancellationToken: cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}