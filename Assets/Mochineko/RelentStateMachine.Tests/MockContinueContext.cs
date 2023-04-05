#nullable enable
namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class MockContinueContext
    {
        public int PhaseCount { get; set; }
        public bool Finished { get; set; }
    }
}