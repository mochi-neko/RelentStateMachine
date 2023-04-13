#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

#pragma warning disable CS1998

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class FinishState : IState<MockContinueEvent, MockContinueContext>
    {
        public async UniTask<IResult<IEventRequest<MockContinueEvent>>> EnterAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            context.Finished = true;
            
            return StateResults.Succeed<MockContinueEvent>();
        }

        public async UniTask<IResult<IEventRequest<MockContinueEvent>>> UpdateAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            return StateResults.Succeed<MockContinueEvent>();
        }

        public async UniTask<IResult> ExitAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            return Results.Succeed();
        }

        public void Dispose()
        {
        }
    }
}