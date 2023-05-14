#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

#pragma warning disable CS1998

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class Phase4State : IState<MockContinueEvent, MockContinueContext>
    {
        public async UniTask<IEventRequest<MockContinueEvent>> EnterAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            context.PhaseCount++;
            
            return EventRequests<MockContinueEvent>.None();
        }

        public async UniTask<IEventRequest<MockContinueEvent>> UpdateAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            return EventRequests<MockContinueEvent>.Request(MockContinueEvent.Stop);
        }

        public async UniTask ExitAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            
        }

        public void Dispose()
        {
        }
    }
}