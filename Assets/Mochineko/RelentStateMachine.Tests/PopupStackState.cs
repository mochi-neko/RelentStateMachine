#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine.Tests
{
#pragma warning disable CS1998
    internal sealed class PopupStackState : IStackState<MockStackContext>
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