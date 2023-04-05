#nullable enable
namespace Mochineko.RelentStateMachine
{
    public static class EventRequestFactory
    {
        public static IEventRequest<TEvent> Request<TEvent>(TEvent @event)
            => new SomeEventRequest<TEvent>(@event);

        public static IEventRequest<TEvent> None<TEvent>()
            => NoEventRequest<TEvent>.Instance;
    }
}