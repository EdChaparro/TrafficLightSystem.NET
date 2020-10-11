using System.Threading;
using IntrepidProducts.TLS.Light;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntrepidProducts.TLS.Tests
{
    [TestClass]
    public class TrafficLightControllerTest
    {
        private TrafficLight _l1;
        private TrafficLight _l2;
        private TrafficLight _l3;
        private TrafficLight _l4;
        private TrafficLight _l5;
        private TrafficLight _l6;

        [TestInitialize]
        public void Initialize()
        {
            _l1 = new TrafficLight {Name = "Test-1"};
            _l2 = new TrafficLight {Name = "Test-2"};
            _l3 = new TrafficLight {Name = "Test-3"};
            _l4 = new TrafficLight {Name = "Test-4"};
            _l5 = new TrafficLight {Name = "Test-5"};
            _l6 = new TrafficLight {Name = "Test-6"};
        }

        private void SetTrafficLightConfiguration(ITrafficLightConfig config)
        {
            _l1.Configuration = config;
            _l2.Configuration = config;
            _l3.Configuration = config;
            _l4.Configuration = config;
            _l5.Configuration = config;
            _l6.Configuration = config;
        }

        [TestMethod]
        public void ShouldSynchronizedAllTetheredLights()
        {
            var lights = new TrafficLights {_l1, _l2, _l3, _l3};
            var timer = new TrafficLightTimer();

            var tc1 = new TetheredTrafficLightCluster { _l1, _l2 };
            tc1.Name = "TC-1";

            var tc2 = new TetheredTrafficLightCluster { _l3, _l4 };
            tc2.Name = "TC-2";

            lights.Add(tc1, tc2);

            var controller = new TrafficLightController(lights, timer);

            //Assert initial state is where expected
            Assert.AreEqual(TrafficLightState.StopThenGo, _l1.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l2.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l3.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l4.State);

            controller.Start();  //Will trigger traffic light state changes
            Thread.Sleep(2500);

            Assert.AreEqual(TrafficLightState.Go, _l1.State);
            Assert.AreEqual(TrafficLightState.Go, _l2.State);
            Assert.AreEqual(TrafficLightState.Go, _l3.State);
            Assert.AreEqual(TrafficLightState.Go, _l4.State);

            controller.Stop();
        }

        [TestMethod]
        public void ShouldSynchronizedAllPolarLights()
        {
            var lights = new TrafficLights { _l1, _l2, _l3, _l4 };
            var timer = new TrafficLightTimer();

            var tc1 = new PolarTrafficLightCluster(_l3) {{_l1, _l2}};
            tc1.Name = "TC-1";

            var tc2 = new PolarTrafficLightCluster(_l5) {{_l4}};
            tc2.Name = "TC-2";

            Assert.IsTrue(lights.Add(tc1, tc2));

            var controller = new TrafficLightController(lights, timer);

            //Assert initial state is where expected
            Assert.AreEqual(TrafficLightState.StopThenGo, _l1.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l2.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l3.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l4.State);

            controller.Start();  //Will trigger traffic light state changes
            Thread.Sleep(2500);

            Assert.AreEqual(TrafficLightState.Go, _l3.State);
            Assert.AreEqual(TrafficLightState.Go, _l5.State);
            Assert.AreEqual(TrafficLightState.Stop, _l1.State);
            Assert.AreEqual(TrafficLightState.Stop, _l2.State);
            Assert.AreEqual(TrafficLightState.Stop, _l4.State);

            controller.Stop();
        }

        [TestMethod]
        public void ShouldStartTrafficLightCycle()
        {
            var configuration = new TrafficLightConfig();
            configuration.Add(TrafficLightState.Go, 4);
            configuration.Add(TrafficLightState.Stop, 4);
            SetTrafficLightConfiguration(configuration);

            var lights = new TrafficLights {_l1, _l2, _l3, _l4};
            var timer = new TrafficLightTimer();

            var tc1 = new TetheredTrafficLightCluster {_l1, _l2};
            tc1.Name = "TC-1";

            var pc1 = new PolarTrafficLightCluster(_l3) {{_l4}};
            pc1.Name = "TC-2";

            Assert.IsTrue(lights.Add(tc1));
            Assert.IsTrue(lights.Add(pc1));

            var controller = new TrafficLightController(lights, timer);

            //Assert initial state is where expected
            Assert.AreEqual(TrafficLightState.StopThenGo, _l1.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l2.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l3.State);
            Assert.AreEqual(TrafficLightState.StopThenGo, _l4.State);

            controller.Start();  //Will trigger traffic light state changes

            Thread.Sleep(2000);
            Assert.AreEqual(TrafficLightState.Go, _l1.State);
            Assert.AreEqual(TrafficLightState.Go, _l2.State);
            Assert.AreEqual(TrafficLightState.Go, _l3.State);
            Assert.AreEqual(TrafficLightState.Stop, _l4.State);

            Thread.Sleep(5000);
            Assert.AreEqual(TrafficLightState.Stop, _l1.State);
            Assert.AreEqual(TrafficLightState.Stop, _l2.State);
            Assert.AreEqual(TrafficLightState.Stop, _l3.State);
            Assert.AreEqual(TrafficLightState.Go, _l4.State);

            controller.Stop();
        }
    }
}