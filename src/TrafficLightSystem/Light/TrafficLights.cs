using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IntrepidProducts.TLS.Light
{
    public class TrafficLights : ITrafficLights
    {
        private readonly Dictionary<ITrafficLight, (TetheredTrafficLightCluster, PolarTrafficLightCluster)>
            _trafficLights = new Dictionary<ITrafficLight, 
                (TetheredTrafficLightCluster TetheredLights,  PolarTrafficLightCluster PolarLights)>();

        private readonly List<TetheredTrafficLightCluster> _tetheredLightClusters = new List<TetheredTrafficLightCluster>();
        private readonly List<PolarTrafficLightCluster> _polarLightClusters = new List<PolarTrafficLightCluster>();

        public bool Add(params ITrafficLight[] trafficLights)
        {
            var itemsToAdd = new Dictionary<ITrafficLight, 
                (TetheredTrafficLightCluster TetheredLights, PolarTrafficLightCluster PolarLights)>();

            foreach (var trafficLight in trafficLights)
            {
                if (_trafficLights.ContainsKey(trafficLight))
                {
                    return false;
                }

                itemsToAdd[trafficLight] = (TetheredLights: null, PolarLights: null);
            }

            itemsToAdd.ToList().ForEach
                (x => _trafficLights.Add(x.Key, x.Value));

            return true;
        }

        public bool Add(params TetheredTrafficLightCluster[] tetheredLightClusters)
        {
            foreach (var cluster in tetheredLightClusters)
            {
                if (cluster.Any(trafficLight => !IsValid(trafficLight, cluster)))
                {
                    return false;
                }
            }

            foreach (var cluster in tetheredLightClusters)
            {
                foreach (var trafficLight in cluster)
                {
                    if (!_trafficLights.ContainsKey(trafficLight))
                    {
                        Add(trafficLight);
                    }

                    (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights) tuple
                        = _trafficLights[trafficLight];

                    tuple.TetheredLights = cluster;
                    SetTuple(trafficLight, tuple);
                }
            }

            return true;
        }

        public bool Add(params PolarTrafficLightCluster[] polarLightClusters)
        {
            foreach (var cluster in polarLightClusters)
            {
                if (!IsValid(cluster))
                {
                    return false;
                }

                if (!_trafficLights.ContainsKey(cluster.MasterTrafficLight))
                {
                    Add(cluster.MasterTrafficLight);
                }

                (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights) tuple
                    = _trafficLights[cluster.MasterTrafficLight];

                tuple.PolarLights = cluster;
                SetTuple(cluster.MasterTrafficLight, tuple);

                foreach (var trafficLight in cluster)
                {
                    if (!_trafficLights.ContainsKey(trafficLight))
                    {
                        Add(trafficLight);
                    }
                }
            }

            return true;
        }

        private bool IsValid(ITrafficLight trafficLight, TetheredTrafficLightCluster cluster)
        {
            if (!_trafficLights.ContainsKey(trafficLight))
            {
                return true;    //Brand New, nothing to do
            }

            (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights) tuple
                = _trafficLights[trafficLight];


            if (_polarLightClusters.Any(x => x.MasterTrafficLight.Equals(trafficLight)))
            {
                return false;   //Polar subjects cannot be members of a Tethered Cluster
            }

            if ((tuple.TetheredLights == null) && (tuple.PolarLights == null))
            {
                return true;    //No clusters assigned to this Traffic Light, nothing to check
            }

            if (tuple.TetheredLights == null)
            {
                return tuple.PolarLights.IsCompatible(cluster); //Ensure this Tether can be paired with this Polar
            }

            if (tuple.TetheredLights.Equals(cluster))
            {
                return true;    //Identical to existing Tethered, benign
            }

            return false;
        }

        private bool IsValid(PolarTrafficLightCluster cluster)
        {
            foreach (var trafficLight in cluster.TetheredTrafficLightCluster)
            {
                var isPolarClusterSubject
                    = _polarLightClusters
                        .Any(x => x.MasterTrafficLight.Equals(trafficLight));

                if (isPolarClusterSubject)
                {
                    return false;   //Polar subject may not be member of another Polar cluster
                }
            }

            if (!_trafficLights.ContainsKey(cluster.MasterTrafficLight))
            {
                return true;    //New Traffic Light, nothing else to validate 
            }

            (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights) tuple
                = _trafficLights[cluster.MasterTrafficLight];

            if ((tuple.TetheredLights == null) && (tuple.PolarLights == null))
            {
                return true;    //No clusters assigned to this Traffic Light, nothing to check
            }

            if (tuple.TetheredLights != null)
            {
                return cluster.IsCompatible(tuple.TetheredLights);  //Ensure this Tether can be paired with this Polar
            }

            if (tuple.PolarLights != null)
            {
                return tuple.PolarLights.Equals(cluster);  //Identical to existing Polar Cluster, benign
            }

            return false;
        }

        private void SetTuple(ITrafficLight trafficLight,
            (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights) tuple)
        {
            _trafficLights[trafficLight] = tuple;

            if (tuple.TetheredLights != null)
            {
                if (!_tetheredLightClusters.Contains(tuple.TetheredLights))
                {
                    _tetheredLightClusters.Add(tuple.TetheredLights);
                }
            }

            if (tuple.PolarLights != null)
            {
                if (!_polarLightClusters.Contains(tuple.PolarLights))
                {
                    _polarLightClusters.Add(tuple.PolarLights);
                }
            }
        }

        #region IEnumerator
        public IEnumerator<ITrafficLight> GetEnumerator() => _trafficLights.Keys.GetEnumerator();

        public IEnumerable<TetheredTrafficLightCluster> AllTetheredClusters => _tetheredLightClusters.ToList();

        public IEnumerable<PolarTrafficLightCluster> AllPolarClusters => _polarLightClusters.ToList();

        public IEnumerable<PolarTrafficLightCluster> AllPolarClustersWithNoPairedTethers
        {
            get
            {
                var tuples = _trafficLights.Values.ToList();

                return tuples.Where(x
                    => x.Item1 == null && x.Item2 != null)
                    .Select(x => x.Item2);
            }
        }

        public (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights) 
            GetClusterFor(Guid trafficLightId)
        {
            var trafficLightInfo
                = _trafficLights.FirstOrDefault(x => x.Key.Id == trafficLightId);

            return trafficLightInfo.Value;
        }

        public (TetheredTrafficLightCluster? TetheredLights, PolarTrafficLightCluster? PolarLights)
            GetClusterFor(ITrafficLight trafficLight)
        {
            return GetClusterFor(trafficLight.Id);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}