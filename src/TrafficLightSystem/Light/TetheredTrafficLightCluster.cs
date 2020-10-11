using System;
using System.Linq;
using IntrepidProducts.TLS.Light.Event;

namespace IntrepidProducts.TLS.Light
{
    public interface ITetheredTrafficLightCluster : ICluster
    {
        event EventHandler<TetheredClusterRequiresUpdateEventArg> ClusterRequiresUpdateEvent;
        ITrafficLight MasterTrafficLight();
        void SetMasterStateTo(TrafficLightState state);
    }

    /// <summary>
    /// Lights whose state must match the inbound signal
    /// </summary>
    public class TetheredTrafficLightCluster : AbstractTrafficLightCluster, ITetheredTrafficLightCluster
    {
        /// <summary>
        /// Traffic Light whose State Changes drive the synching of the cluster
        /// </summary>
        /// <returns></returns>
        public ITrafficLight MasterTrafficLight()
        {
            return TrafficLights.FirstOrDefault();
        }

        public void SetMasterStateTo(TrafficLightState state)
        {
            foreach (var light in TrafficLights)
            {
                light.State = state;
            }
        }

        protected override void AfterAdd(ITrafficLight trafficLight)
        {
            trafficLight.StateExpirationEvent += OnTrafficLightStateExpired;
        }

        public event EventHandler<TetheredClusterRequiresUpdateEventArg> ClusterRequiresUpdateEvent;

        private void OnTrafficLightStateExpired(object sender, TrafficLightStateExpirationEventArgs e)
        {
            if (e.TrafficLightId == MasterTrafficLight()?.Id)
            {
                RaiseClusterRequiresUpdateEvent(e.State);
            }
        }

        private void RaiseClusterRequiresUpdateEvent(TrafficLightState state)
        {
            ClusterRequiresUpdateEvent?.Invoke(this,
                new TetheredClusterRequiresUpdateEventArg(this, state));
        }
    }
}