#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class Phase3State : IState<MockContinueEvent, MockContinueContext>
    {
        public async UniTask<IResult<IEventRequest<MockContinueEvent>>> EnterAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(0.01f),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                return StateResultFactory.Fail<MockContinueEvent>(exception.Message);
            }
            
            context.PhaseCount++;

            return StateResultFactory.SucceedAndRequest(MockContinueEvent.Continue);
        }

        public async UniTask<IResult<IEventRequest<MockContinueEvent>>> UpdateAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(0.01f),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                return StateResultFactory.Fail<MockContinueEvent>(exception.Message);
            }

            return StateResultFactory.Succeed<MockContinueEvent>();
        }

        public async UniTask<IResult> ExitAsync(
            MockContinueContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(0.01f),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                return ResultFactory.Fail(exception.Message);
            }

            return ResultFactory.Succeed();
        }

        public void Dispose()
        {
        }
    }
}