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
    internal sealed class ContinuousStateMachineTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public async Task StateMachineShouldTransitByEvent()
        {
            // NOTE: Initial state is specified at constructor.
            var transitionMapBuilder = TransitionMapBuilder<MockContinueEvent, MockContinueContext>
                .Create<Phase1State>();

            // NOTE: Register sequential transitions to builder.
            transitionMapBuilder.RegisterTransition<Phase1State, Phase2State>(MockContinueEvent.Continue);
            transitionMapBuilder.RegisterTransition<Phase2State, Phase3State>(MockContinueEvent.Continue);
            transitionMapBuilder.RegisterTransition<Phase3State, Phase4State>(MockContinueEvent.Continue);
            transitionMapBuilder.RegisterTransition<Phase4State, FinishState>(MockContinueEvent.Stop);
            
            using var stateMachine = (await FiniteStateMachine<MockContinueEvent, MockContinueContext>.CreateAsync(
                transitionMapBuilder.Build(),
                new MockContinueContext(),
                CancellationToken.None))
                .Unwrap();

            stateMachine.IsCurrentState<Phase4State>()
                .Should().BeTrue(
                    because: "Initial state should be run to phase 1 ~ 4 state by sending event on enter state.");

            stateMachine.Context.PhaseCount
                .Should().Be(4);

            await stateMachine.UpdateAsync(CancellationToken.None);
            
            stateMachine.IsCurrentState<FinishState>()
                .Should().BeTrue(
                    because: "Phase 4 state should be run to finish state by sending event on update state.");

            stateMachine.Context.Finished
                .Should().BeTrue();
        }
    }
}