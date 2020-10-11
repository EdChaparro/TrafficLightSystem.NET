using System.Threading;
using IntrepidProducts.TLS.Light;

namespace IntrepidProducts.TLS
{
    public interface ITrafficLightService
    {
        void Start();
        void Stop();
    }

    public class TrafficLightController : ITrafficLightService
    {
        public TrafficLightController(ITrafficLights lights, ITrafficLightTimer timer)
        {
            _lights = lights;
            _timer = timer;

        }

        private readonly ITrafficLights _lights;
        private readonly ITrafficLightTimer _timer;
        private Thread _timerThread;

        public void Start()
        {
            StartTimer();
        }

        public void Stop()
        {
            _timer.Stop();

            if (_timerThread.IsAlive)
            {
                _timerThread.Join();        //Wait for shutdown to complete
            }
        }

        private void StartTimer()
        {
            _timer.Init(_lights);

            _timerThread = new Thread(_timer.Start)
            {
                Name = "TrafficLightTimerThread"
            };

            _timerThread.Start();
        }
    }
}