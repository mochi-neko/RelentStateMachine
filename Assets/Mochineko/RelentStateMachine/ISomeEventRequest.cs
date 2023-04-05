#nullable enable
namespace Mochineko.RelentStateMachine
{
    public interface ISomeEventRequest<TEvent> : IEventRequest<TEvent>
    {
        TEvent Event { get; }
    }
}