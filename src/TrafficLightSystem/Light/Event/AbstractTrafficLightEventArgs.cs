using System;

namespace IntrepidProducts.TLS.Light.Event
{
    public abstract class AbstractTrafficLightEventArgs : EventArgs
    {
        protected AbstractTrafficLightEventArgs(Guid trafficLightId)
        {
            TrafficLightId = trafficLightId;
        }

        public Guid TrafficLightId { get; }
    }
}