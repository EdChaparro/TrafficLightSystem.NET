using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IntrepidProducts.TLS.Light;
using IntrepidProducts.TLS.Light.Event;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntrepidProducts.TLS.Tests.Light
{
    [TestClass]
    public class PolarTrafficLightClusterTest
    {
        #region Equality Tests
        [TestMethod]
        public void ShouldTreatPolarClustersWithIdenticalAttributesAsEqual()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();
            var t3 = new TrafficLight();

            var cluster1 = new PolarTrafficLightCluster(t1) { t2, t3 };
            var cluster2 = new PolarTrafficLightCluster(t1) { t2, t3 };
            Assert.AreEqual(cluster1, cluster2);

            cluster2.Add(new TrafficLight());
            Assert.AreNotEqual(cluster1, cluster2);
        }

        [TestMethod]
        public void ShouldTreatPolarClustersWithDifferentSubjectTrafficLightAsNotEqual()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();
            var t3 = new TrafficLight();
            var t4 = new TrafficLight();

            var cluster1 = new PolarTrafficLightCluster(t1) { t2, t3 };
            var cluster2 = new PolarTrafficLightCluster(t4) { t2, t3 };
            Assert.AreNotEqual(cluster1, cluster2);
        }
        #endregion

        #region Validate Polar Traffic Lights
        [TestMethod]
        public void ShouldNotPermitSubjectTrafficLightToParticipateInCluster()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();
            var t3 = new TrafficLight();

            var cluster = new PolarTrafficLightCluster(t1) { t2, t3 };
            Assert.IsFalse(cluster.Add(t1));
        }

        [TestMethod]
        public void ShouldNotAcceptAnInvalidPolarTrafficLight()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();

            var cluster = new PolarTrafficLightCluster(t1) { t2, t1 };

            Assert.AreEqual(1, cluster.GetTrafficLightIds().Count());
            Assert.AreEqual(t2.Id, cluster.GetTrafficLightIds().First());
        }
        #endregion

        #region Compatible Tethers Tests
        [TestMethod]
        public void ShouldConsiderTethersWithOverlappingMembersAsInCompatible()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();
            var t3 = new TrafficLight();

            var polarCluster = new PolarTrafficLightCluster(t1) { t2, t3 };
            var tetheredCluster = new TetheredTrafficLightCluster { t2, t3 };

            Assert.IsFalse(polarCluster.IsCompatible(tetheredCluster));
        }

        [TestMethod]
        public void ShouldConsiderTethersCompatibleWhenThereAreNoCommonMembers()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();
            var t3 = new TrafficLight();
            var t4 = new TrafficLight();

            var polarCluster = new PolarTrafficLightCluster(t1) {t2, t3};
            var tetheredCluster = new TetheredTrafficLightCluster {t4};

            Assert.IsTrue(polarCluster.IsCompatible(tetheredCluster));

            tetheredCluster.Add(t2);
            Assert.IsFalse(polarCluster.IsCompatible(tetheredCluster));
        }

        #endregion

        [TestMethod]
        public void ShouldProduceTetheredClusterOfAllPolarLights()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();
            var t3 = new TrafficLight();

            var cluster = new PolarTrafficLightCluster(t1) { t2, t3 };

            var tetheredCluster = cluster.TetheredTrafficLightCluster;

            Assert.IsNotNull(tetheredCluster);

            Assert.AreEqual(2, tetheredCluster.GetTrafficLightIds().Count());
            Assert.IsFalse(tetheredCluster.Except(new List<ITrafficLight> { t2, t3 }).Any());
        }

        [TestMethod]
        public void ShouldUpdateClusterStateWhenRequested()
        {
            var t1 = new TrafficLight();
            var t2 = new TrafficLight();

            var cluster = new PolarTrafficLightCluster(t1) {t2};

            Assert.AreEqual(TrafficLightState.StopThenGo, t1.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, t2.State);

            cluster.SetMasterStateTo(TrafficLightState.Go);
            Assert.AreEqual(TrafficLightState.Go, t1.State);
            Assert.AreEqual(TrafficLightState.Stop, t2.State);
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

            var cluster = new PolarTrafficLightCluster(light);

            var receivedEvents = new List<PolarClusterRequiresUpdateEventArg>();

            cluster.ClusterRequiresUpdateEvent += (sender, e)
                => receivedEvents.Add(e);

            light.State = TrafficLightState.Go;
            Thread.Sleep(4000);

            Assert.AreEqual(1, receivedEvents.Count);
        }
    }
}