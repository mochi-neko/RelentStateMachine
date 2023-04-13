#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class ErrorState : IState<MockEvent, MockContext>
    {
        public async UniTask<IResult<IEventRequest<MockEvent>>> EnterAsync(MockContext context,
            CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);

            context.ErrorMessage = "Manual Error";

            return Results.Succeed(EventRequests.None<MockEvent>());
        }

        public async UniTask<IResult<IEventRequest<MockEvent>>> UpdateAsync(MockContext context,
            CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);

            return Results.Succeed(EventRequests.None<MockEvent>());
        }

        public async UniTask<IResult> ExitAsync(MockContext context, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1, cancellationToken: cancellationToken);

            return Results.Succeed();
        }

        public void Dispose()
        {
        }
    }
}