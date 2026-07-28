using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration
{

    public class OverviewCameraSettings
    {
        public string Host { get; set; }
        public string PictureUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Enabled { get; set; }

        public int RefreshIntervalMs { get; set; } = 700; // default
    }

}
