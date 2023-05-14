#nullable enable
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    internal static class StateResults
    {
        public static ISuccessResult<IEventRequest<TEvent>> Succeed<TEvent>()
            => Results.Succeed(EventRequests.None<TEvent>());
     
        public static ISuccessResult<IEventRequest<TEvent>> SucceedAndRequest<TEvent>(TEvent @event)
            => Results.Succeed(EventRequests.Request(@event));
        
        public static IFailureResult<IEventRequest<TEvent>> Fail<TEvent>(string message)
            => Results.FailWithTrace<IEventRequest<TEvent>>(message);
    }
}