#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class Phase1State : IState<MockContinueEvent, MockContinueContext>
    {
        public async UniTask<IEventRequest<MockContinueEvent>> EnterAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(0.01f),
                cancellationToken: cancellationToken);

            context.PhaseCount++;

            return EventRequests.Request(MockContinueEvent.Continue);
        }

        public async UniTask<IEventRequest<MockContinueEvent>> UpdateAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            await UniTask.Delay(
                    TimeSpan.FromSeconds(0.01f),
                    cancellationToken: cancellationToken);
            
            return EventRequests.None<MockContinueEvent>();
        }

        public async UniTask ExitAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
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