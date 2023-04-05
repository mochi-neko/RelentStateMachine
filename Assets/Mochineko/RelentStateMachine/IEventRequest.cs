#nullable enable
namespace Mochineko.RelentStateMachine
{
    public interface IEventRequest<TEvent>
    {
        bool Request { get; }
    }
}