using System;
using System.Linq;
using IntrepidProducts.TLS.Light.Event;

namespace IntrepidProducts.TLS.Light
{
    public interface IPolarTrafficLightCluster : ICluster
    {
        ITrafficLight MasterTrafficLight { get; }
        TetheredTrafficLightCluster TetheredTrafficLightCluster { get; }
        
        /// <param name="tetheredCluster"></param>
        /// <returns>True if the argument cluster can be paired with this Polar Cluster</returns>
        bool IsCompatible(TetheredTrafficLightCluster tetheredCluster);
        void SetMasterStateTo(TrafficLightState state);

        event EventHandler<PolarClusterRequiresUpdateEventArg> ClusterRequiresUpdateEvent;
    }

    /// <summary>
    /// Lights whose state must oppose the subject traffic light
    /// </summary>
    public class PolarTrafficLightCluster : AbstractTrafficLightCluster, IPolarTrafficLightCluster
    {
        public PolarTrafficLightCluster(ITrafficLight masterTrafficLight)
        {
            MasterTrafficLight = masterTrafficLight;
            masterTrafficLight.StateExpirationEvent += OnTrafficLightStateExpired;
        }

        public ITrafficLight MasterTrafficLight { get; }

        public void SetMasterStateTo(TrafficLightState state)
        {
            MasterTrafficLight.State = state;
            var polarState = TrafficLight.GetPolarState(state);

            foreach (var light in TrafficLights)
            {
                light.State = polarState;
            }
        }

        private TetheredTrafficLightCluster _tetheredTrafficLightCluster;
        public TetheredTrafficLightCluster TetheredTrafficLightCluster
        {
            get
            {
                if (_tetheredTrafficLightCluster != null)
                {
                    return _tetheredTrafficLightCluster;
                }

                _tetheredTrafficLightCluster = new TetheredTrafficLightCluster {TrafficLights.ToArray()};
                return _tetheredTrafficLightCluster;
            }
        }

        public override bool Add(params ITrafficLight[] trafficLights)
        {
            if (trafficLights.Any(x => x.Equals(MasterTrafficLight)))
            {
                return false;
            }

            return base.Add(trafficLights);
        }

        public bool IsCompatible(TetheredTrafficLightCluster tetheredCluster)
        {
            if (tetheredCluster.Equals(TetheredTrafficLightCluster))
            {
                return false;
            }

            var tetheredTrafficIds = tetheredCluster.GetTrafficLightIds().ToList();

            //if (tetheredTrafficIds.Contains(TrafficLight.Id))
            //{
            //    return false;
            //}

            var polarTrafficIds = TrafficLights.Select(x => x.Id);

            return !polarTrafficIds.Intersect(tetheredTrafficIds).Any();
        }

        public event EventHandler<PolarClusterRequiresUpdateEventArg> ClusterRequiresUpdateEvent;

        private void OnTrafficLightStateExpired(object sender, TrafficLightStateChangedEventArgs e)
        {
            if (e.TrafficLightId == MasterTrafficLight.Id)
            {
                RaiseClusterRequiresUpdateEvent(e.State);
            }
        }

        private void RaiseClusterRequiresUpdateEvent(TrafficLightState state)
        {
            ClusterRequiresUpdateEvent?.Invoke(this,
                new PolarClusterRequiresUpdateEventArg(this, state));
        }

        #region Equals
    public bool Equals(PolarTrafficLightCluster cluster)
        {
            return this.MasterTrafficLight.Equals(cluster.MasterTrafficLight) && 
                   IsMember(cluster.GetTrafficLightIds().ToArray());
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((PolarTrafficLightCluster) obj);
        }

        public override int GetHashCode()
        {
            return MasterTrafficLight.GetHashCode();
        }
        #endregion
    }
}