using System.Collections.Generic;
using System.Threading;
using IntrepidProducts.TLS.Light;
using IntrepidProducts.TLS.Light.Event;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntrepidProducts.TLS.Tests.Light
{
    [TestClass]
    public class TetheredTrafficLightClusterTest
    {
        #region Equality Tests
        [TestMethod]
        public void ShouldTreatTetheredClustersWithIdenticalSetsAsEqual()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();

            var cluster1 = new TetheredTrafficLightCluster {t1, t2};
            var cluster2 = new TetheredTrafficLightCluster {t1, t2};
            Assert.AreEqual(cluster1, cluster2);

            cluster2.Add(new TrafficLight());
            Assert.AreNotEqual(cluster1, cluster2);
        }

        [TestMethod]
        public void ShouldTreatEmptyTetheredClustersAsEqual()
        {
            var cluster1 = new TetheredTrafficLightCluster();
            var cluster2 = new TetheredTrafficLightCluster();
            Assert.AreEqual(cluster1, cluster2);
        }

        [TestMethod]
        public void ShouldTreatDistinctClustersTypesAsNotEqual()
        {
            var cluster1 = new TetheredTrafficLightCluster();
            var cluster2 = new PolarTrafficLightCluster(new TrafficLight());
            Assert.AreNotEqual(cluster1, cluster2);
        }
        #endregion

        [TestMethod]
        public void ShouldUpdateClusterStateWhenRequested()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();

            var cluster = new TetheredTrafficLightCluster { t1, t2 };
            Assert.AreEqual(TrafficLightState.StopThenGo, t1.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, t2.State);

            cluster.SetMasterStateTo(TrafficLightState.Go);
            Assert.AreEqual(TrafficLightState.Go, t1.State);
            Assert.AreEqual(TrafficLightState.Go, t2.State);
        }

        [TestMethod]
        public void ShouldRaiseEventWhenClusterRequiresUpdating()
        {
            var configuration = new TrafficLightConfig();
            configuration.Add(TrafficLightState.Go, 3);

            var light = new TrafficLight
            {
                Name = "State Expired Test Light",
                Configuration = configuration
            };

            var cluster = new TetheredTrafficLightCluster {light};

            var receivedEvents = new List<TetheredClusterRequiresUpdateEventArg>();

            cluster.ClusterRequiresUpdateEvent += (sender, e)
                => receivedEvents.Add(e);

            light.State = TrafficLightState.Go;
            Thread.Sleep(4000);

            Assert.AreEqual(1, receivedEvents.Count);
        }
    }
}