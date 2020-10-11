using System;
using System.Collections.Generic;
using IntrepidProducts.TLS.Light.Event;

namespace IntrepidProducts.TLS.Light
{
    public interface ICluster : IEnumerable<ITrafficLight>, IEquatable<ICluster>
    {
        Guid Id { get; }
        string? Name { get; set; }

        bool IsMember(params Guid[] trafficLightIds);

        bool Add(params ITrafficLight[] trafficLights);

        IEnumerable<Guid> GetTrafficLightIds();
    }
}