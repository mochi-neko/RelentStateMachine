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
    internal sealed class StateMachineTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public async Task StateMachineShouldTransitByEvent()
        {
            // NOTE: Initial state is specified at constructor.
            var transitionMapBuilder = TransitionMapBuilder<MockEvent, MockContext>
                .Create<InactiveState>();

            // NOTE: Register transitions to builder.
            transitionMapBuilder.RegisterTransition<InactiveState, ActiveState>(MockEvent.Activate);
            transitionMapBuilder.RegisterTransition<ActiveState, InactiveState>(MockEvent.Deactivate);
            transitionMapBuilder.RegisterAnyTransition<ErrorState>(MockEvent.Fail);

            // NOTE: Built transition map is immutable.
            IStateMachine<MockEvent, MockContext> stateMachine;
            var initializeResult = await StateMachine<MockEvent, MockContext>.CreateAsync(
                transitionMapBuilder.Build(),
                new MockContext(),
                CancellationToken.None);
            if (initializeResult is ISuccessResult<StateMachine<MockEvent, MockContext>> initializeSuccess)
            {
                stateMachine = initializeSuccess.Result;
            }
            else if (initializeResult is IFailureResult<StateMachine<MockEvent, MockContext>> initializeFailure)
            {
                throw new System.Exception(
                    $"Failed to initialize state machine because of {initializeFailure.Message}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(initializeResult));
            }

            // Inactive state ------------------------------------------------------

            // NOTE: Asynchronously created state machine has been already in initial state.
            stateMachine.IsCurrentState<InactiveState>()
                .Should().BeTrue(
                    because: $"Initial state should be {nameof(InactiveState)}.");

            stateMachine.Context.Active
                .Should().BeFalse();

            await stateMachine.UpdateAsync(CancellationToken.None);

            stateMachine.IsCurrentState<InactiveState>()
                .Should().BeTrue(
                    because: $"State should not change on update.");

            (await stateMachine
                    .SendEventAsync(MockEvent.Deactivate, CancellationToken.None))
                .Failure.Should().BeTrue(
                    because: $"Already in {nameof(InactiveState)}.");

            // Active state ------------------------------------------------------
            
            (await stateMachine
                    .SendEventAsync(MockEvent.Activate, CancellationToken.None))
                .Success.Should().BeTrue(
                    because:
                    $"Can change state from {nameof(InactiveState)} to {nameof(ActiveState)} by {MockEvent.Activate}.");

            stateMachine.IsCurrentState<ActiveState>()
                .Should().BeTrue();

            stateMachine.Context.Active
                .Should().BeTrue();

            await stateMachine.UpdateAsync(CancellationToken.None);

            (await stateMachine
                    .SendEventAsync(MockEvent.Activate, CancellationToken.None))
                .Failure.Should().BeTrue(
                    because: $"Already in {nameof(ActiveState)}.");

            stateMachine.IsCurrentState<ActiveState>()
                .Should().BeTrue(
                    because: "State should not change by sending event failure.");

            stateMachine.Context.Active
                .Should().BeTrue();

            // Inactive state ------------------------------------------------------
            
            (await stateMachine
                    .SendEventAsync(MockEvent.Deactivate, CancellationToken.None))
                .Success.Should().BeTrue(
                    because:
                    $"Can change state from {nameof(ActiveState)} to {nameof(ActiveState)} by {MockEvent.Deactivate}.");

            stateMachine.IsCurrentState<InactiveState>()
                .Should().BeTrue();

            stateMachine.Context.Active
                .Should().BeFalse();

            await stateMachine.UpdateAsync(CancellationToken.None);

            // Error state ------------------------------------------------------
            
            (await stateMachine
                    .SendEventAsync(MockEvent.Fail, CancellationToken.None))
                .Success.Should().BeTrue(
                    because: $"Error state can be transited from any state by {MockEvent.Fail}.");

            stateMachine.IsCurrentState<ErrorState>()
                .Should().BeTrue();

            stateMachine.Context.ErrorMessage
                .Should().Be("Manual Error");
        }
        
        [Test]
        [RequiresPlayMode(false)]
        public void BuilderShouldNotBeReused()
        {
            var transitionMapBuilder = TransitionMapBuilder<MockEvent, MockContext>
                .Create<InactiveState>();

            transitionMapBuilder.RegisterTransition<InactiveState, ActiveState>(MockEvent.Activate);
            transitionMapBuilder.RegisterTransition<ActiveState, InactiveState>(MockEvent.Deactivate);
            
            _ = transitionMapBuilder.Build();

            Action operateBuilder = () => transitionMapBuilder
                .RegisterAnyTransition<ErrorState>(MockEvent.Fail);

            operateBuilder.Should().Throw<ObjectDisposedException>();
        }
    }
}