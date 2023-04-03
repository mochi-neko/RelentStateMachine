#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class InactiveState : IState<MockContext>
    {
        async UniTask<IResult> IState<MockContext>.EnterAsync(
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
                return ResultFactory.Fail(exception.Message);
            }

            context.Active = false;

            return ResultFactory.Succeed();
        }

        public async UniTask<IResult> UpdateAsync(MockContext context, CancellationToken cancellationToken)
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
                return ResultFactory.Fail(exception.Message);
            }

            return ResultFactory.Succeed();
        }

        public void Dispose()
        {
        }
    }
}