using System.Linq;
using IntrepidProducts.TLS.Light;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntrepidProducts.TLS.Tests.Light
{
    [TestClass]
    public class TrafficLightsTest
    {
        [TestMethod]
        public void ShouldNotAcceptDuplicateTrafficLights()
        {
            var lights = new TrafficLights();

            var l1 = new TrafficLight();
            var l2 = new TrafficLight();

            Assert.IsTrue(lights.Add(l1));
            Assert.IsTrue(lights.Add(l2));
            Assert.IsFalse(lights.Add(l1));

            Assert.AreEqual(2, lights.Count());
        }

        [TestMethod]
        public void ShouldNotPermitTrafficLightsToParticipateInMultipleTetheredClusters()
        {
            var lights = new TrafficLights();

            var l1 = new TrafficLight();
            var l2 = new TrafficLight();
            var l3 = new TrafficLight();
            var l4 = new TrafficLight();
            var l5 = new TrafficLight();

            var t1 = new TetheredTrafficLightCluster {l1, l2}; //l1 assigned to Tethered Cluster
            var t2 = new TetheredTrafficLightCluster {l3, l4};
            var t3 = new TetheredTrafficLightCluster {l5, l1}; //l1 already assigned to Tethered Cluster

            Assert.IsTrue(lights.Add(t1));
            Assert.IsTrue(lights.Add(t2));
            Assert.IsFalse(lights.Add(t3));

            Assert.IsNull(lights.GetClusterFor(l5.Id).TetheredLights);

            Assert.AreEqual(4, lights.Count()); //l5 not added due to invalid cluster
            Assert.AreEqual(2, lights.AllTetheredClusters.Count());  

            Assert.AreEqual(lights.GetClusterFor(l5.Id), (null, null));
        }

        [TestMethod]
        public void ShouldNotPermitTrafficLightsToParticipateInMultiplePolarClusters()
        {
            var lights = new TrafficLights();

            var l1 = new TrafficLight { Name = "TL-1" };
            var l2 = new TrafficLight { Name = "TL-2" };
            var l3 = new TrafficLight { Name = "TL-3" };
            var l4 = new TrafficLight { Name = "TL-4" };
            var l5 = new TrafficLight { Name = "TL-5" };
            var l6 = new TrafficLight { Name = "TL-6" };

            var t1 = new PolarTrafficLightCluster(l3) {l1, l2}; 
            t1.Name = "Polar Cluster 1";

            var t2 = new PolarTrafficLightCluster(l5) {l4};
            t2.Name = "Polar Cluster 2";

            var t3 = new PolarTrafficLightCluster(l5) {l6}; //l5 already assigned to Polar Cluster
            t3.Name = "Polar Cluster 3";

            Assert.IsTrue(lights.Add(t1));
            Assert.IsTrue(lights.Add(t2));
            Assert.IsFalse(lights.Add(t3));

            Assert.AreEqual(5, lights.Count()); //Traffic lights in invalid clusters not added
            Assert.AreEqual(2, lights.AllPolarClusters.Count());  //l3 & l5 Polar Clusters
        }

        [TestMethod]
        public void ShouldPermitTrafficLightToBelongToTetherAndPolarClusters()
        {
            var lights = new TrafficLights();

            var l1 = new TrafficLight { Name = "TL-1" };
            var l2 = new TrafficLight { Name = "TL-2" };
            var l3 = new TrafficLight { Name = "TL-3" };
            var l4 = new TrafficLight { Name = "TL-4" };

            var t1 = new TetheredTrafficLightCluster {{l1, l2}};      //l1 belongs to 
            var t2 = new PolarTrafficLightCluster(l1) { l3, l4 };   //  both clusters.

            Assert.IsTrue(lights.Add(t1));
            Assert.IsTrue(lights.Add(t2));

            Assert.AreEqual(4, lights.Count()); //All unique traffic lights added
            Assert.AreEqual(1, lights.AllTetheredClusters.Count());
            Assert.AreEqual(1, lights.AllPolarClusters.Count());
        }

        [TestMethod]
        public void ShouldNotPermitConflictingTetheredAndPolarClusters()
        {
            var lights = new TrafficLights();

            var l1 = new TrafficLight();
            var l2 = new TrafficLight();
            var l3 = new TrafficLight();
            var l4 = new TrafficLight();
            var l5 = new TrafficLight();
            var l6 = new TrafficLight();

            var t1 = new TetheredTrafficLightCluster { l2, l3 };  // I2 is Tethered to I3, but 
            var t2 = new PolarTrafficLightCluster(l3) { l1, l2, };    // also defined as a l3 Polar -- illogical
            Assert.IsTrue(lights.Add(t1));
            Assert.IsFalse(lights.Add(t2));

            var t3 = new TetheredTrafficLightCluster { l3, l4, l5 };    // Overlapping Traffic Lights,
            var t4 = new PolarTrafficLightCluster(l6) { l4, l5, };          //  this is not allowed.

            Assert.IsTrue(lights.Add(t4));  //Polar cluster first
            Assert.IsFalse(lights.Add(t3)); //  to ensure validation works regardless of order
        }

        [TestMethod]
        public void ShouldNotPermitPolarSubjectsToBelongToTethers()
        {
            var lights = new TrafficLights();

            var l1 = new TrafficLight { Name = "TL-1" };
            var l2 = new TrafficLight { Name = "TL-2" };
            var l3 = new TrafficLight { Name = "TL-3" };
            var l4 = new TrafficLight { Name = "TL-4" };

            var t1 = new PolarTrafficLightCluster(l1) { l2, l3, }; 
            var t2 = new TetheredTrafficLightCluster { l1, l4 };  
            Assert.IsTrue(lights.Add(t1));
            Assert.IsFalse(lights.Add(t2));
        }

        [TestMethod]
        public void ShouldNotPermitPolarSubjectsToBeMembersOfOtherPolarClusters()
        {
            var lights = new TrafficLights();

            var l1 = new TrafficLight { Name = "TL-1" };
            var l2 = new TrafficLight { Name = "TL-2" };
            var l3 = new TrafficLight { Name = "TL-3" };
            var l4 = new TrafficLight { Name = "TL-4" };
            var l5 = new TrafficLight { Name = "TL-5" };

            var t1 = new PolarTrafficLightCluster(l1) { l2, l3, };
            var t2 = new PolarTrafficLightCluster(l4) { l1, l5, };
            Assert.IsTrue(lights.Add(t1));
            Assert.IsFalse(lights.Add(t2));
        }
    }
}