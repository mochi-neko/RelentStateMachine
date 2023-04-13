#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class InactiveState : IState<MockEvent, MockContext>
    {
        async UniTask<IResult<IEventRequest<MockEvent>>> IState<MockEvent, MockContext>.EnterAsync(
            MockContext context,
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
                return StateResults.Fail<MockEvent>(exception.Message);
            }

            context.Active = false;

            return StateResults.Succeed<MockEvent>();
        }

        public async UniTask<IResult<IEventRequest<MockEvent>>> UpdateAsync(MockContext context, CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(0.01f),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                return StateResults.Fail<MockEvent>(exception.Message);
            }

            return StateResults.Succeed<MockEvent>();
        }

        public async UniTask<IResult> ExitAsync(MockContext context, CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(0.01f),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                return Results.Fail(exception.Message);
            }

            return Results.Succeed();
        }

        public void Dispose()
        {
        }
    }
}