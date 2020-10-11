using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IntrepidProducts.TLS.Light.Event;

namespace IntrepidProducts.TLS.Light
{
    /// <summary>
    /// Grouping of one or more Traffic Lights that are tightly coordinated.
    /// </summary>
    public abstract class AbstractTrafficLightCluster : ICluster
    {
        public Guid Id { get; } = Guid.NewGuid();

        private IList<ITrafficLight> _trafficLights = new List<ITrafficLight>();

        protected IList<ITrafficLight> TrafficLights => _trafficLights.ToList();

        public string? Name { get; set; } = "No Name";

        public bool IsMember(params Guid[] trafficLightIds)
        {
            foreach (var id in trafficLightIds)
            {
                if (_trafficLights.All(x => x.Id != id))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool Add(params ITrafficLight[] trafficLights)
        {
            var mergeLights = new List<ITrafficLight>(_trafficLights);
            mergeLights.AddRange(trafficLights);

            var duplicates = mergeLights.GroupBy(x => x)
                .Where(y => y.Count() > 1)
                .Select(z => z.GetHashCode());

            if (duplicates.Any())
            {
                return false;
            }

            _trafficLights = mergeLights;

            foreach (var trafficLight in trafficLights)
            {
                AfterAdd(trafficLight);
            }

            return true;
        }

        protected virtual void AfterAdd(ITrafficLight trafficLight)
        {}

        public IEnumerable<Guid> GetTrafficLightIds()
        {
            return _trafficLights.Select(x => x.Id);
        }

        #region IEnumerator
        public IEnumerator<ITrafficLight> GetEnumerator()
        {
            return _trafficLights.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Equals
        public bool Equals(ICluster other)
        {
            return GetTrafficLightIds().Count() == other.GetTrafficLightIds().Count() && 
                   IsMember(other.GetTrafficLightIds().ToArray());
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

            return Equals((ICluster) obj);
        }

        public override int GetHashCode()
        {
            return _trafficLights.GetHashCode();
        }
        #endregion

        public override string ToString()
        {
            return $"Name: {Name}";
        }
    }
}