#nullable enable
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class MockStateMachineController : MonoBehaviour
    {
        private StateMachine<MockEvent, MockContext>? stateMachine;

        private async void Awake()
        {
            var transitionMapBuilder = TransitionMapBuilder<MockEvent, MockContext>
                .Create<InactiveState>();

            transitionMapBuilder.RegisterTransition<InactiveState, ActiveState>(MockEvent.Activate);
            transitionMapBuilder.RegisterTransition<ActiveState, InactiveState>(MockEvent.Deactivate);
            transitionMapBuilder.RegisterTransition<InactiveState, ErrorState>(MockEvent.Fail);
            transitionMapBuilder.RegisterTransition<ActiveState, ErrorState>(MockEvent.Fail);

            var initializeResult = await StateMachine<MockEvent, MockContext>.CreateAsync(
                transitionMapBuilder.Build(),
                new MockContext(),
                this.GetCancellationTokenOnDestroy());
            if (initializeResult is ISuccessResult<StateMachine<MockEvent, MockContext>> initializeSuccess)
            {
                stateMachine = initializeSuccess.Result;
            }
            else
            {
                throw new System.Exception("Failed to initialize state machine.");
            }
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

            var result = await stateMachine.UpdateAsync(
                this.GetCancellationTokenOnDestroy());
            if (result.Success)
            {
                Debug.Log("Succeeded to update.");
            }
            else
            {
                Debug.Log("Failed to update.");
            }
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