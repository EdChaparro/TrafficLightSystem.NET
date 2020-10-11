using System;
using System.Collections.Generic;

namespace IntrepidProducts.TLS.Light
{
    public interface ITrafficLights : IEnumerable<ITrafficLight>
    {
        bool Add(params ITrafficLight[] trafficLights);
        bool Add(params TetheredTrafficLightCluster[] tetheredLightCluster);
        bool Add(params PolarTrafficLightCluster[] polarLightCluster);

        IEnumerable<TetheredTrafficLightCluster> AllTetheredClusters { get; }
        IEnumerable<PolarTrafficLightCluster> AllPolarClusters { get; }

        IEnumerable<PolarTrafficLightCluster> AllPolarClustersWithNoPairedTethers { get; }

        (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights)
            GetClusterFor(Guid trafficLightId);

        (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights)
            GetClusterFor(ITrafficLight trafficLight);
    }
}