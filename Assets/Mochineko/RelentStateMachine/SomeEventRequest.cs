#nullable enable
namespace Mochineko.RelentStateMachine
{
    internal sealed class SomeEventRequest<TEvent> : ISomeEventRequest<TEvent>
    {
        public bool Request => true;
        public TEvent Event { get; }

        public SomeEventRequest(TEvent @event)
        {
            Event = @event;
        }
    }
}