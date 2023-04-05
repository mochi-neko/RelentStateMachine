#nullable enable
using Mochineko.Relent.Result;

namespace Mochineko.RelentStateMachine
{
    public static class StateResultFactory
    {
        public static ISuccessResult<IEventRequest<TEvent>> Succeed<TEvent>()
            => ResultFactory.Succeed(EventRequestFactory.None<TEvent>());
     
        public static ISuccessResult<IEventRequest<TEvent>> SucceedAndRequest<TEvent>(TEvent @event)
            => ResultFactory.Succeed(EventRequestFactory.Request(@event));
        
        public static IFailureResult<IEventRequest<TEvent>> Fail<TEvent>(string message)
            => ResultFactory.Fail<IEventRequest<TEvent>>(message);
    }
}