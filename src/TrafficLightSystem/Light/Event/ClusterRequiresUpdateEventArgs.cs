using System;

namespace IntrepidProducts.TLS.Light.Event
{
    public abstract class AbstractClusterRequiresUpdateEventArgs<T> : EventArgs where T : class, ICluster
    {
        /// <summary>
        /// Event includes the Cluster requiring a State update.
        /// The second parameter reports the state of the "master" traffic light driving the
        /// cluster.
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="masterLightState"></param>
        /// <
        protected AbstractClusterRequiresUpdateEventArgs(T cluster, TrafficLightState masterLightState)
        {
            Cluster = cluster;
            MasterLightState = masterLightState;
        }

        public T Cluster { get; }
        public TrafficLightState MasterLightState { get; }
    }

    public class TetheredClusterRequiresUpdateEventArg
        : AbstractClusterRequiresUpdateEventArgs<TetheredTrafficLightCluster>
    {
        public TetheredClusterRequiresUpdateEventArg(TetheredTrafficLightCluster cluster, TrafficLightState masterLightState) 
            : base(cluster, masterLightState)
        {}
    }

    public class PolarClusterRequiresUpdateEventArg
        : AbstractClusterRequiresUpdateEventArgs<PolarTrafficLightCluster>
    {
        public PolarClusterRequiresUpdateEventArg(PolarTrafficLightCluster cluster, TrafficLightState masterLightState) 
            : base(cluster, masterLightState)
        {}
    }
}