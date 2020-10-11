using System.Collections.Generic;
using System.Threading;
using IntrepidProducts.TLS.Light;
using IntrepidProducts.TLS.Light.Event;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntrepidProducts.TLS.Tests.Light
{
    [TestClass]
    public class TrafficLightTest
    {
        [TestMethod]
        public void ShouldHaveDefaultConfiguration()
        {
            Assert.IsNotNull(new TrafficLight().Configuration);
        }

        [TestMethod]
        public void ShouldDefaultToStopThenGoState()
        {
            Assert.AreEqual(TrafficLightState.StopThenGo,  new TrafficLight().State);
        }

        [TestMethod]
        public void ShouldRaiseEventWhenStateExpires()
        {
            var configuration = new TrafficLightConfig();
            configuration.Add(TrafficLightState.Go, 3);

            var light = new TrafficLight
            {
                Name = "State Expired Test Light",
                Configuration = configuration
            };

            var receivedEvents = new List<TrafficLightStateExpirationEventArgs>();

            light.StateExpirationEvent += (sender, e)
                => receivedEvents.Add(e);

            light.State = TrafficLightState.Go;
            Thread.Sleep(4000);

            Assert.AreEqual(1, receivedEvents.Count);
            Assert.AreEqual(light.Id, receivedEvents[0].TrafficLightId);
        }

        [TestMethod]
        public void ShouldRaiseEventWhenStateChanges()
        {
            var light = new TrafficLight();
            var receivedEvents = new List<TrafficLightStateChangedEventArgs>();

            light.StateChangedEvent += (sender, e) 
                => receivedEvents.Add(e);

            light.State = TrafficLightState.Go;

            Assert.AreEqual(1, receivedEvents.Count);
            Assert.AreEqual(light.Id, receivedEvents[0].TrafficLightId);
            Assert.AreEqual(light.State, receivedEvents[0].State);
        }

        [TestMethod]
        public void ShouldRevertToSafeStateAfterStateTimerExpires()
        {
            var config = new TrafficLightConfig();
            config.Add(TrafficLightState.Go, 1); //Set to one second

            var light = new TrafficLight {Configuration = config, State = TrafficLightState.Go};

            Assert.AreEqual(TrafficLightState.Go, light.State);

            Thread.Sleep(7000);    //Wait for fail-safe to kick in

            Assert.AreEqual(TrafficLightState.StopThenGo, light.State);
        }
    }
}