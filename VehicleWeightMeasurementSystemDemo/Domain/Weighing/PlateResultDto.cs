using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Domain.Weighing
{
    public class PlateResultDto
    {
        public string PlateNumber { get; set; }
        public float Confidence { get; set; }
        public string Country { get; set; }
        public Bitmap PlateImage { get; set; }

        public byte Direction { get; set; } // DIR_UNKNOWN=0, DIR_COMING=1, DIR_DEPARTING=2
    }
}
