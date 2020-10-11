using System;
using IntrepidProducts.TLS.Light.Event;

namespace IntrepidProducts.TLS.Light
{
    public interface ITrafficLight
    {
        Guid Id { get; }
        string? Name { get; set; }

        TrafficLightState State { get; set; }
        DateTime StateExpiration { get; }

        event EventHandler<TrafficLightStateChangedEventArgs> StateChangedEvent;
        event EventHandler<TrafficLightStateExpirationEventArgs> StateExpirationEvent;

        ITrafficLightConfig Configuration { get; set; } 

        //TODO: Consider adding other properties
        //Direction FacingDirection { get; }
        //string Road { get; }
        //decimal Latitude { get; }
        //decimal Longitude { get; }
    }
}