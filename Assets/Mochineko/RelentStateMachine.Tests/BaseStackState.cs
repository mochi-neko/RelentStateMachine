#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

#pragma warning disable CS1998

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class BaseStackState : IStackState<MockStackContext>
    {
        public async UniTask EnterAsync(MockStackContext context, CancellationToken cancellationToken)
        {
        }

        public async UniTask UpdateAsync(MockStackContext context, CancellationToken cancellationToken)
        {
            
        }

        public async UniTask ExitAsync(MockStackContext context, CancellationToken cancellationToken)
        {
            
        }
        
        public void Dispose()
        {
        }
    }
}