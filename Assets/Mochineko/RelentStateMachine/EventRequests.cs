#nullable enable
using System.Collections.Generic;

namespace Mochineko.RelentStateMachine
{
    public static class EventRequests<TEvent>
    {
        public static IEventRequest<TEvent> Request(TEvent @event)
        {
            if (requestsCache.TryGetValue(@event, out var request))
            {
                return request;
            }
            else
            {
                var newInstance = new SomeEventRequest<TEvent>(@event);
                requestsCache.Add(@event, newInstance);
                return newInstance;
            }
        }

        public static IEventRequest<TEvent> None()
            => NoEventRequest<TEvent>.Instance;

        private static readonly Dictionary<TEvent, SomeEventRequest<TEvent>> requestsCache = new();
    }
}