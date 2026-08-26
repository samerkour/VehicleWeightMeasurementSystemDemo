using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Domain.Entities
{
    public class VehicleEntity
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? PlateNumber { get; set; }

        public double? Speed { get; set; }
        public double? AverageSpeed { get; set; }

        public int? AxleCount { get; set; }
        public double? TotalWeight { get; set; }


        public string? ADC1 { get; set; }
        public string? ADC2 { get; set; }
        public string? ADC3 { get; set; }
        public string? ADC4 { get; set; }


        public int? LineId { get; set; }
        public LineEntity? Line { get; set; }

        public double? PlateConfidence { get; set; }
        public int? PlateReadStatus { get; set; }
        public DateTime? PlateReadAt { get; set; }

        public bool? Allowed { get; set; }
        public bool? WrongDirection { get; set; }

        public int? VehicleClass { get; set; }

        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public double? VehicleLen { get; set; }
        public double TotalOverWeight { get; set; }

        public int? SpeedType { get; set; }
        public string? CrimeCodes { get; set; }
        public int? HeadGap { get; set; }
        public int? Gap { get; set; }

        public string? WimRawLine { get; set; }

        // 🔥 Navigation
        public List<AxleEntity> Axles { get; set; } = new();
        public List<CameraPhotosEntity> Photos { get; set; } = new();
    }
}
