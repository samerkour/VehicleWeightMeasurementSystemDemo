using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo
{
    public class CameraSettings
    {
        public string FolderPath { get; set; }
        public string Filter { get; set; } = "*.jpg";
        public bool IncludeSubfolders { get; set; } = false;
    }
}
