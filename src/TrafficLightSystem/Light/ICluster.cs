using System;
using System.Collections.Generic;

namespace IntrepidProducts.TLS.Light
{
    public interface ICluster : IEnumerable<ITrafficLight>, IEquatable<ICluster>
    {
        string? Name { get; set; }

        bool IsMember(params Guid[] trafficLightIds);

        bool Add(params ITrafficLight[] trafficLights);

        IEnumerable<Guid> GetTrafficLightIds();
    }
}