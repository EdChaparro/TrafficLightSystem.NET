using System;
using System.Threading;
using IntrepidProducts.TLS.Light;
using IntrepidProducts.TLS.Light.Event;

namespace IntrepidProducts.TLS
{
    public interface ITrafficLightTimer : ITrafficLightService
    {
        void Init(ITrafficLights lights);
    }

    public class TrafficLightTimer : ITrafficLightTimer
    {

        public TrafficLightTimer()
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
        }

        private ITrafficLights _lights;
        protected bool IsInitialized { get; set; }
        protected bool IsRunning { get; set; }
        protected bool IsStopRequested { get; set; }

        protected CancellationTokenSource CancellationTokenSource { get; private set; }
        protected CancellationToken CancellationToken { get; private set; }

        protected int SleepIntervalInMilliseconds { get; set; } = 2000;

        public void Init(ITrafficLights lights)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Init called while Engine Running");
            }

            _lights = lights;
            IsStopRequested = false;

            IsInitialized = true;
        }

        public void Start()
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;

            if (!IsInitialized)
            {
                throw new InvalidOperationException("Thread worker is not initialized");
            }

            BeforeEngineStart();
            IsRunning = true;

            while (IsStopRequested == false)
            {
                DoEngineLoop();
                bool isCancelled = CancellationToken.WaitHandle.WaitOne(SleepIntervalInMilliseconds);

                if (isCancelled)
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Thread worker is not initialized");
            }

            CancellationTokenSource.Cancel();
            IsStopRequested = true;     
            DoStop();
            IsInitialized = false;
            IsRunning = false;
        }

        protected virtual void BeforeEngineStart()
        {
            SetClusterObservability();
            StartTrafficLightSignalCycle();
        }

        protected virtual void DoEngineLoop()
        {}

        protected virtual void DoStop()
        {}

        private void SetClusterObservability()
        {
            foreach (var cluster in _lights.AllTetheredClusters)
            {
                cluster.ClusterRequiresUpdateEvent += OnTetheredClusterRequiresUpdate;
            }

            foreach (var cluster in _lights.AllPolarClustersWithNoPairedTethers)
            {
                cluster.ClusterRequiresUpdateEvent += OnPolarClusterRequiresUpdate;
            }
        }

        private void OnTetheredClusterRequiresUpdate(object sender, TetheredClusterRequiresUpdateEventArg e)
        {
            e.Cluster.SetMasterStateTo(TrafficLight.GetPolarState(e.MasterLightState));

            var tuple = _lights.GetClusterFor(e.Cluster.MasterTrafficLight());

            tuple.PolarLights?.SetMasterStateTo(e.MasterLightState);
        }

        private void OnPolarClusterRequiresUpdate(object sender, PolarClusterRequiresUpdateEventArg e)
        {
            e.Cluster.SetMasterStateTo(TrafficLight.GetPolarState(e.MasterLightState));
        }

        private void StartTrafficLightSignalCycle()
        {
            foreach (var cluster in _lights.AllTetheredClusters)
            {
                cluster.SetMasterStateTo(TrafficLightState.Go);

                var tuple = _lights.GetClusterFor(cluster.MasterTrafficLight());

                tuple.PolarLights?.SetMasterStateTo(TrafficLight.GetPolarState(TrafficLightState.Go));
            }

            foreach (var cluster in _lights.AllPolarClustersWithNoPairedTethers)
            {
                cluster.SetMasterStateTo(TrafficLightState.Go);
            }
        }
    }
}