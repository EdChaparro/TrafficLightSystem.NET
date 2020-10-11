using System;

namespace IntrepidProducts.TLS.Light.Event
{
    public class TrafficLightStateChangedEventArgs : AbstractTrafficLightEventArgs
    {
        public TrafficLightStateChangedEventArgs(Guid trafficLightId, TrafficLightState state) :
            base(trafficLightId)
        {
            State = state;
        }

        public TrafficLightState State { get; }
    }
}