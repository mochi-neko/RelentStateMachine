#nullable enable
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
        public async Task Test()
        {
            var transitionMap = StateStore<MockEvent, MockContext>
                .Create<InactiveState>();

            transitionMap.AddTransition<InactiveState, ActiveState>(MockEvent.Activate);
            transitionMap.AddTransition<ActiveState, InactiveState>(MockEvent.Deactivate);
            transitionMap.AddTransition<InactiveState, ErrorState>(MockEvent.Fail);
            transitionMap.AddTransition<ActiveState, ErrorState>(MockEvent.Fail);

            IStateMachine<MockEvent, MockContext> stateMachine;
            var initializeResult = await StateMachine<MockEvent, MockContext>.CreateAsync(
                transitionMap,
                new MockContext(),
                CancellationToken.None);
            if (initializeResult is ISuccessResult<StateMachine<MockEvent, MockContext>> initializeSuccess)
            {
                stateMachine = initializeSuccess.Result;
            }
            else
            {
                throw new System.Exception("Failed to initialize state machine.");
            }

            // Inactive state ------------------------------------------------------

            stateMachine.IsCurrentState<InactiveState>()
                .Should().BeTrue(
                    because: "Initial state should be InactiveState.");

            stateMachine.Context.Active
                .Should().BeFalse();

            await stateMachine.UpdateAsync(CancellationToken.None);

            stateMachine.IsCurrentState<InactiveState>()
                .Should().BeTrue(
                    because: "State should not change on update.");

            (await stateMachine
                    .SendEventAsync(MockEvent.Deactivate, CancellationToken.None))
                .Failure.Should().BeTrue(
                    because: "Already in inactive state.");

            (await stateMachine
                    .SendEventAsync(MockEvent.Activate, CancellationToken.None))
                .Success.Should().BeTrue(
                    because: "Can change state from inactive to active by activate event.");

            // Active state ------------------------------------------------------

            stateMachine.IsCurrentState<ActiveState>()
                .Should().BeTrue();

            stateMachine.Context.Active
                .Should().BeTrue();

            await stateMachine.UpdateAsync(CancellationToken.None);

            (await stateMachine
                    .SendEventAsync(MockEvent.Activate, CancellationToken.None))
                .Failure.Should().BeTrue(
                    because: "Already in active state.");

            stateMachine.IsCurrentState<ActiveState>()
                .Should().BeTrue(
                    because: "State should not change by adding event.");

            stateMachine.Context.Active
                .Should().BeTrue();

            (await stateMachine
                    .SendEventAsync(MockEvent.Deactivate, CancellationToken.None))
                .Success.Should().BeTrue(
                    because: "Can change state from active to inactive by deactivate event.");

            // Inactive state ------------------------------------------------------

            stateMachine.IsCurrentState<InactiveState>()
                .Should().BeTrue();

            stateMachine.Context.Active
                .Should().BeFalse();

            await stateMachine.UpdateAsync(CancellationToken.None);
        }
    }
}