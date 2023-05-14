#nullable enable
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class MockStateMachineController : MonoBehaviour
    {
        private FiniteStateMachine<MockEvent, MockContext>? stateMachine;

        private async void Awake()
        {
            var transitionMapBuilder = TransitionMapBuilder<MockEvent, MockContext>
                .Create<InactiveState>();

            transitionMapBuilder.RegisterTransition<InactiveState, ActiveState>(MockEvent.Activate);
            transitionMapBuilder.RegisterTransition<ActiveState, InactiveState>(MockEvent.Deactivate);
            transitionMapBuilder.RegisterTransition<InactiveState, ErrorState>(MockEvent.Fail);
            transitionMapBuilder.RegisterTransition<ActiveState, ErrorState>(MockEvent.Fail);

            stateMachine = await FiniteStateMachine<MockEvent, MockContext>
                .CreateAsync(
                    transitionMapBuilder.Build(),
                    new MockContext(),
                    this.GetCancellationTokenOnDestroy());
        }

        private void OnDestroy()
        {
            if (stateMachine is null)
            {
                throw new System.NullReferenceException(nameof(stateMachine));
            }

            stateMachine.Dispose();
        }

        private async void Update()
        {
            if (stateMachine is null)
            {
                throw new System.NullReferenceException(nameof(stateMachine));
            }

            await stateMachine.UpdateAsync(this.GetCancellationTokenOnDestroy());
        }

        public async UniTask Activate()
        {
            if (stateMachine is null)
            {
                throw new System.NullReferenceException(nameof(stateMachine));
            }

            var result = await stateMachine.SendEventAsync(
                MockEvent.Activate,
                this.GetCancellationTokenOnDestroy());
            if (result.Success)
            {
                Debug.Log("Succeeded to activated.");
                Assert.IsTrue(stateMachine.Context.Active);
            }
            else
            {
                Debug.Log("Failed to activate.");
                Assert.IsFalse(stateMachine.Context.Active);
            }
        }

        public async UniTask Deactivate()
        {
            if (stateMachine is null)
            {
                throw new System.NullReferenceException(nameof(stateMachine));
            }

            var result = await stateMachine.SendEventAsync(
                MockEvent.Deactivate,
                this.GetCancellationTokenOnDestroy());
            if (result.Success)
            {
                Debug.Log("Succeeded to deactivated.");
                Assert.IsFalse(stateMachine.Context.Active);
            }
            else
            {
                Debug.Log("Failed to deactivate.");
                Assert.IsTrue(stateMachine.Context.Active);
            }
        }

        public async UniTask Fail()
        {
            if (stateMachine is null)
            {
                throw new System.NullReferenceException(nameof(stateMachine));
            }

            var result = await stateMachine.SendEventAsync(
                MockEvent.Fail,
                this.GetCancellationTokenOnDestroy());
            if (result.Success)
            {
                Debug.Log("Succeeded to deactivated.");
                Assert.IsFalse(stateMachine.Context.Active);
            }
            else
            {
                Debug.Log("Failed to deactivate.");
                Assert.IsTrue(stateMachine.Context.Active);
            }
        }
    }
}