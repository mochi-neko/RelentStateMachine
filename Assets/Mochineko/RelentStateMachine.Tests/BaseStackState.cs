#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;
#pragma warning disable CS1998

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class BaseStackState : IStackState<MockStackContext>
    {
        public void Dispose()
        {
        }

        public async UniTask<IResult> EnterAsync(MockStackContext context, CancellationToken cancellationToken)
        {
            return ResultFactory.Succeed();
        }

        public async UniTask<IResult> UpdateAsync(MockStackContext context, CancellationToken cancellationToken)
        {
            return ResultFactory.Succeed();
        }

        public async UniTask<IResult> ExitAsync(MockStackContext context, CancellationToken cancellationToken)
        {
            return ResultFactory.Succeed();
        }
    }
}