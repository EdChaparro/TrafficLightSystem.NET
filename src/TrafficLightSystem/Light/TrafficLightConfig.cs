using System;
using System.Collections.Generic;

namespace IntrepidProducts.TLS.Light
{
    public interface ITrafficLightConfig
    {
        ITrafficLightConfig Add(TrafficLightState state, int durationInSeconds);
        DateTime CalculateExpiration(TrafficLightState state);
        int GetTimeDuration(TrafficLightState state);
    }

    public class TrafficLightConfig : ITrafficLightConfig
    {
        public TrafficLightConfig()
        {
            Add(TrafficLightState.Go, 60);
            Add(TrafficLightState.Stop, 60);
            Add(TrafficLightState.Transitioning, 5);

            Add(TrafficLightState.StopThenGo, int.MaxValue);
        }

        private readonly IDictionary<TrafficLightState, int> _stateTimeDurations = new Dictionary<TrafficLightState, int>();

        public ITrafficLightConfig Add(TrafficLightState state, int durationInSeconds)
        {
            _stateTimeDurations[state] = durationInSeconds;
            return this;
        }

        public DateTime CalculateExpiration(TrafficLightState state)
        {
            return DateTime.Now.AddSeconds(GetTimeDuration(state));
        }

        public int GetTimeDuration(TrafficLightState state)
        {
            return _stateTimeDurations[state];
        }

        private static ITrafficLightConfig _default;
        public static ITrafficLightConfig Default() => _default ??= new TrafficLightConfig();
    }
}