#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mochineko.Relent.Result;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mochineko.RelentStateMachine.Tests
{
    [TestFixture]
    internal sealed class StackStateMachineTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public async Task StateMachineShouldTransitByEvent()
        {
            // NOTE: Initial state is specified at constructor.
            var stateStore = StateStoreBuilder<MockStackContext>
                .Create<BaseStackState>();

            // NOTE: Register states to builder.
            stateStore.Register<PopupStackState>();
            stateStore.Register<OverlayStackState>();
            stateStore.Register<FirstStackState>();

            // NOTE: Built store is immutable.
            IStackStateMachine<MockStackContext> stateMachine;
            var initializeResult = await StackStateMachine<MockStackContext>.CreateAsync(
                stateStore.Build(),
                new MockStackContext(),
                CancellationToken.None);
            if (initializeResult is ISuccessResult<StackStateMachine<MockStackContext>> initializeSuccess)
            {
                stateMachine = initializeSuccess.Result;
            }
            else if (initializeResult is IFailureResult<StackStateMachine<MockStackContext>> initializeFailure)
            {
                throw new System.Exception(
                    $"Failed to initialize stack state machine because of {initializeFailure.Message}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(initializeResult));
            }

            using var _ = stateMachine;

            // [Base state] ------------------------------------------------------

            // NOTE: Asynchronously created state machine has been already in initial state.
            stateMachine.IsCurrentState<BaseStackState>()
                .Should().BeTrue(
                    because: $"Initial state should be {nameof(BaseStackState)}.");

            stateMachine.Context.PopTokenStack.Count
                .Should().Be(0);
            
            await stateMachine.UpdateAsync(CancellationToken.None);

            // [Popup] ------------------------------------------------------
            // [Base] ------------------------------------------------------

            var pushResult = await stateMachine
                .PushAsync<PopupStackState>(CancellationToken.None);
            pushResult.Success
                .Should().BeTrue();
            if (pushResult is ISuccessResult<IPopToken> pushSuccess)
            {
                stateMachine.Context.PopTokenStack
                    .Push(pushSuccess.Result);
            }
            else
            {
                Assert.Fail();
            }
            
            stateMachine.IsCurrentState<PopupStackState>()
                .Should().BeTrue();
            
            stateMachine.Context.PopTokenStack.Count
                .Should().Be(1);

            await stateMachine.UpdateAsync(CancellationToken.None);

            // [Overlay] ------------------------------------------------------
            // [Popup] ------------------------------------------------------
            // [Base] ------------------------------------------------------

            pushResult = await stateMachine
                .PushAsync<OverlayStackState>(CancellationToken.None);
            pushResult.Success
                .Should().BeTrue();
            if (pushResult is ISuccessResult<IPopToken> pushSuccess2)
            {
                stateMachine.Context.PopTokenStack
                    .Push(pushSuccess2.Result);
            }
            else
            {
                Assert.Fail();
            }
            
            stateMachine.IsCurrentState<OverlayStackState>()
                .Should().BeTrue();
            
            stateMachine.Context.PopTokenStack.Count
                .Should().Be(2);

            await stateMachine.UpdateAsync(CancellationToken.None);
            
            // [Popup] ------------------------------------------------------
            // [Overlay] ------------------------------------------------------
            // [Popup] ------------------------------------------------------
            // [Base] ------------------------------------------------------

            pushResult = await stateMachine
                .PushAsync<FirstStackState>(CancellationToken.None);
            pushResult.Success
                .Should().BeTrue();
            if (pushResult is ISuccessResult<IPopToken> pushSuccess3)
            {
                stateMachine.Context.PopTokenStack
                    .Push(pushSuccess3.Result);
            }
            else
            {
                Assert.Fail();
            }
            
            stateMachine.IsCurrentState<FirstStackState>()
                .Should().BeTrue();
            
            stateMachine.Context.PopTokenStack.Count
                .Should().Be(3);

            await stateMachine.UpdateAsync(CancellationToken.None);
            
            // [Overlay] ------------------------------------------------------
            // [Popup] ------------------------------------------------------
            // [Base] ------------------------------------------------------
            
            var firstPopToken = stateMachine.Context.PopTokenStack.Pop();
            await firstPopToken.PopAsync(CancellationToken.None);
            
            stateMachine.IsCurrentState<OverlayStackState>()
                .Should().BeTrue();
            
            stateMachine.Context.PopTokenStack.Count
                .Should().Be(2);
            
            await stateMachine.UpdateAsync(CancellationToken.None);
            
            // [Popup] ------------------------------------------------------
            // [Base] ------------------------------------------------------
            
            var overlayPopToken = stateMachine.Context.PopTokenStack.Pop();
            await overlayPopToken.PopAsync(CancellationToken.None);
            
            stateMachine.IsCurrentState<PopupStackState>()
                .Should().BeTrue();
            
            stateMachine.Context.PopTokenStack.Count
                .Should().Be(1);
            
            // [First] ------------------------------------------------------
            // [Popup] ------------------------------------------------------
            // [Base] ------------------------------------------------------
            
            pushResult = await stateMachine
                .PushAsync<FirstStackState>(CancellationToken.None);
            pushResult.Success
                .Should().BeTrue();
            if (pushResult is ISuccessResult<IPopToken> pushSuccess4)
            {
                stateMachine.Context.PopTokenStack
                    .Push(pushSuccess4.Result);
            }
            else
            {
                Assert.Fail();
            }
            
            stateMachine.IsCurrentState<FirstStackState>()
                .Should().BeTrue();
            
            stateMachine.Context.PopTokenStack.Count
                .Should().Be(2);
            
            // [Base] ------------------------------------------------------

            while (stateMachine.Context.PopTokenStack.TryPop(out var popToken))
            {
                await popToken.PopAsync(CancellationToken.None);
            }
            
            stateMachine.IsCurrentState<BaseStackState>()
                .Should().BeTrue();
            
            stateMachine.Context.PopTokenStack.Count
                .Should().Be(0);
        }

        [Test]
        [RequiresPlayMode(false)]
        public void BuilderShouldNotBeReused()
        {
            var stateStoreBuilder = StateStoreBuilder<MockStackContext>
                .Create<BaseStackState>();

            stateStoreBuilder.Register<PopupStackState>();
            stateStoreBuilder.Register<OverlayStackState>();
            stateStoreBuilder.Register<FirstStackState>();

            _ = stateStoreBuilder.Build();

            Action operateBuilder = () => stateStoreBuilder
                .Register<BaseStackState>();

            operateBuilder.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        [RequiresPlayMode(false)]
        public void InitialStateShouldHaveBeenRegistered()
        {
            var stateStoreBuilder = StateStoreBuilder<MockStackContext>
                .Create<BaseStackState>();

            Action registerInitial = () => stateStoreBuilder
                .Register<BaseStackState>();

            registerInitial
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        [RequiresPlayMode(false)]
        public void CannotRegisterSameStateTwice()
        {
            var stateStoreBuilder = StateStoreBuilder<MockStackContext>
                .Create<BaseStackState>();

            stateStoreBuilder.Register<PopupStackState>();

            Action registerTwice = () => stateStoreBuilder
                .Register<PopupStackState>();

            registerTwice
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        [RequiresPlayMode(false)]
        public async Task StackOperationsShouldBeThreadSafe()
        {
            var stateStore = StateStoreBuilder<MockStackContext>
                .Create<BaseStackState>();

            stateStore.Register<PopupStackState>();
            stateStore.Register<OverlayStackState>();
            stateStore.Register<FirstStackState>();

            IStackStateMachine<MockStackContext> stateMachine;
            var initializeResult = await StackStateMachine<MockStackContext>.CreateAsync(
                stateStore.Build(),
                new MockStackContext(),
                CancellationToken.None);
            if (initializeResult is ISuccessResult<StackStateMachine<MockStackContext>> initializeSuccess)
            {
                stateMachine = initializeSuccess.Result;
            }
            else if (initializeResult is IFailureResult<StackStateMachine<MockStackContext>> initializeFailure)
            {
                throw new System.Exception(
                    $"Failed to initialize stack state machine because of {initializeFailure.Message}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(initializeResult));
            }

            using var _ = stateMachine;

#pragma warning disable CS4014
            var firstTask = Task.Run(async () =>
            {
                await stateMachine
                    .PushAsync<PopupStackState>(CancellationToken.None);
            });
            var secondTask = Task.Run(async () =>
            {
                await stateMachine
                    .PushAsync<OverlayStackState>(CancellationToken.None);
            });
#pragma warning restore CS4014

            firstTask.Status.Should().Be(TaskStatus.WaitingForActivation);
            secondTask.Status.Should().Be(TaskStatus.WaitingForActivation);

            await Task.WhenAll(firstTask, secondTask);

            firstTask.Status.Should().Be(TaskStatus.RanToCompletion);
            secondTask.Status.Should().Be(TaskStatus.RanToCompletion);

            stateMachine.IsCurrentState<OverlayStackState>()
                .Should().BeTrue();
        }
    }
}