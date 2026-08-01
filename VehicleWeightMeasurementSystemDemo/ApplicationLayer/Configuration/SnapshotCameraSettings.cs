using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration
{
    public class SnapshotCameraSettings
    {
        public string WatchRootPath { get; set; }
        public string Filter { get; set; }
        public bool IncludeSubfolders { get; set; }
        public bool Enabled { get; set; }
        public int ImageLookbackMs { get; set; } = 1500;
        public int ImageWaitTimeoutMs { get; set; } = 2000;
    }
}
