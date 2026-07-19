using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Camera
{
    public class PlateResultDto
    {
        public string PlateNumber { get; set; }
        public float Confidence { get; set; }
        public string Country { get; set; }
        //public Bitmap PlateImage { get; set; }
    }
}
