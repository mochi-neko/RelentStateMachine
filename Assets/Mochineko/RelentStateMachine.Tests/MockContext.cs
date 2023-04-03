#nullable enable
namespace Mochineko.RelentStateMachine.Tests
{
    internal sealed class MockContext
    {
        public bool Active { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}