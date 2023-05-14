#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

#pragma warning disable CS1998

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class FinishState : IState<MockContinueEvent, MockContinueContext>
    {
        public async UniTask<IEventRequest<MockContinueEvent>> EnterAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            context.Finished = true;
            
            return EventRequests.None<MockContinueEvent>();
        }

        public async UniTask<IEventRequest<MockContinueEvent>> UpdateAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            return EventRequests.None<MockContinueEvent>();
        }

        public UniTask ExitAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}