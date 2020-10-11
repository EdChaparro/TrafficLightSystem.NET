using System;

namespace IntrepidProducts.TLS.Light.Event
{
    public class TrafficLightStateExpirationEventArgs : TrafficLightStateChangedEventArgs
    {
        public TrafficLightStateExpirationEventArgs
            (Guid trafficLightId, TrafficLightState state, DateTime stateExpiration)
            :base(trafficLightId, state)
        {
            StateExpiration = stateExpiration;
        }

        public DateTime StateExpiration { get; }
    }
}