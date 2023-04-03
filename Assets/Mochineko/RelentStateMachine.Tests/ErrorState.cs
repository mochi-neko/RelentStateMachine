#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class ErrorState : IState<MockContext>
    {
        public async UniTask<IResult> EnterAsync(MockContext context, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            
            context.ErrorMessage = "Manual Error";
            
            return ResultFactory.Succeed();
        }

        public async UniTask<IResult> UpdateAsync(MockContext context, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            
            return ResultFactory.Succeed();
        }

        public async UniTask<IResult> ExitAsync(MockContext context, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            
            return ResultFactory.Succeed();
        }
        
        public void Dispose()
        {
        }
    }
}