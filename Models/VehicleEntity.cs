using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Models
{
    public class VehicleEntity
    {
        public int Id { get; set; }

        public string? PlateNumber { get; set; }

        public double Speed { get; set; }
        public int Line { get; set; }
        public int AxleCount { get; set; }
        public double TotalWeight { get; set; }

        public string ADC1 { get; set; }
        public string ADC2 { get; set; }
        public string ADC3 { get; set; }
        public string ADC4 { get; set; }

        public DateTime Timestamp { get; set; }

        public List<AxleEntity> Axles { get; set; } = new();
    }
}
