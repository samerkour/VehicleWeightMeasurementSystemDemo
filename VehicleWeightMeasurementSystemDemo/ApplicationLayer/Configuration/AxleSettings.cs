using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration
{
    public class AxleSettings
    {
        /// <summary>
        /// ضریب اعمال‌شده روی وزن محورها (w1..w6) قبل از نمایش.
        /// </summary>
        public decimal Alpha { get; set; } = 1.0m;
    }
}