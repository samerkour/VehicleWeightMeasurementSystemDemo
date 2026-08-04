using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration
{
    public class WeightSettings
    {
        /// <summary>
        /// ضریب اعمال‌شده روی همهٔ مقادیر وزن (w1..w6 و TotalWeight) قبل از نمایش.
        /// </summary>
        public decimal Alpha { get; set; } = 1.5m;
    }
}