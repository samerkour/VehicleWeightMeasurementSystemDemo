using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Models
{
    public class AxleDto
    {
        public int Index { get; set; }           // Axle number (1..6)

        public double? Weight { get; set; }       // kg
        public double? TimeMs { get; set; }       // milliseconds
        public double? Distance { get; set; }     // meters (calculated)

        // 🔹 Optional: for UI formatting
        public string DisplayTime => TimeMs > 0 ? TimeMs?.ToString("F1") : "--";
        public string DisplayDistance => Distance > 0 ? Distance?.ToString("F2") : "--";
    }
}
