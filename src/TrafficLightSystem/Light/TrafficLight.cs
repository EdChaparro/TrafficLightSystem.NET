using System;
using System.Threading.Tasks;
using IntrepidProducts.TLS.Light.Event;

namespace IntrepidProducts.TLS.Light
{
    public class TrafficLight : ITrafficLight
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? Name { get; set; } = "No Name";

        public ITrafficLightConfig Configuration { get; set; } = TrafficLightConfig.Default();

        public event EventHandler<TrafficLightStateChangedEventArgs> StateChangedEvent;
        public event EventHandler<TrafficLightStateExpirationEventArgs> StateExpirationEvent;

        #region State Management
        private TrafficLightState _state = TrafficLightState.StopThenGo;

        public TrafficLightState State
        {
            get => _state;

            set
            {
                _state = value;
                StateExpiration = Configuration.CalculateExpiration(value);
                RaiseStateChangedEvent();

                if (_state != TrafficLightState.StopThenGo) //Safe mode requires no safety check
                {
                    FailSafeCheck(StateExpiration);
                }
            }
        }

        public DateTime StateExpiration { get; private set; }

        private void RaiseStateChangedEvent()
        {
            StateChangedEvent?.Invoke(this,
                new TrafficLightStateChangedEventArgs(Id, State));
        }

        private void RaiseStateExpirationEvent()
        {
            StateExpirationEvent?.Invoke(this,
                new TrafficLightStateExpirationEventArgs(Id, State, StateExpiration));
        }

        private readonly TimeSpan _failSafeDelayFactor 
            = new TimeSpan(0, 0, 0, 5, 0);

        private async void FailSafeCheck(DateTime stateExpiration)
        {
            var now = DateTime.Now;
            if (stateExpiration > now)  //This may not be true when debugging
            {
                var timeSpan = (StateExpiration - now);
                await Task.Delay(timeSpan);

                if (StateExpiration == stateExpiration)
                {
                    RaiseStateExpirationEvent();
                }

                await Task.Delay(_failSafeDelayFactor);
            }

            if (StateExpiration == stateExpiration)
            {
                //Expiration date hasn't been reset, revert to Fail Safe State 
                State = TrafficLightState.StopThenGo;
            }
        }
        #endregion

        public static TrafficLightState GetPolarState(TrafficLightState state)
        {
            return state switch
            {
                TrafficLightState.Go => TrafficLightState.Stop,
                TrafficLightState.Stop => TrafficLightState.Go,
                TrafficLightState.Transitioning => TrafficLightState.Transitioning,
                _ => TrafficLightState.StopThenGo
            };
        }

        #region Equality
        public override bool Equals(object obj)
        {
            return obj is TrafficLight trafficLight && Equals(trafficLight);
        }

        protected bool Equals(TrafficLight other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion

        public override string ToString()
        {
            return $"Name: {Name}, State: {State}, Id: {Id}";
        }
    }
}