using System;

namespace Fov_Menu
{
    [Serializable]
    public class FovSettings
    {
        public double ChaseMin { get; set; }
        public double ChaseMax { get; set; }
        public double FarChaseMin { get; set; }
        public double FarChaseMax { get; set; }
        public double DriverMin { get; set; }
        public double DriverMax { get; set; }
        public double HoodMin { get; set; }
        public double HoodMax { get; set; }
        public double BumperMin { get; set; }
        public double BumperMax { get; set; }
    }
}
