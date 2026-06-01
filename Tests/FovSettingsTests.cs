using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Fov_Menu;

namespace FovMenu.Tests
{
    [TestClass]
    public class FovSettingsTests
    {
        [TestMethod]
        public void SerializeAndDeserialize_Works()
        {
            var settings = new FovSettings
            {
                ChaseMin = 1.1,
                ChaseMax = 2.2,
                FarChaseMin = 3.3,
                FarChaseMax = 4.4,
                DriverMin = 5.5,
                DriverMax = 6.6,
                HoodMin = 7.7,
                HoodMax = 8.8,
                BumperMin = 9.9,
                BumperMax = 10.1
            };

            var json = JsonSerializer.Serialize(settings);
            var copy = JsonSerializer.Deserialize<FovSettings>(json);

            Assert.IsNotNull(copy);
            Assert.AreEqual(settings.ChaseMin, copy!.ChaseMin);
            Assert.AreEqual(settings.BumperMax, copy.BumperMax);
        }
    }
}
