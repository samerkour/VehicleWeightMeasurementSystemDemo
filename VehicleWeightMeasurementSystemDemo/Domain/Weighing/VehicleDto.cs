using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Domain.Weighing
{
    public class VehicleDto
    {
        public string? PlateNumber { get; set; }

        public double? Speed { get; set; }        // km/h
        public int LineId { get; set; }
        public string? LineName { get; set; }
        public int? AxleCount { get; set; }

        public string? ADC1 { get; set; }
        public string? ADC2 { get; set; }
        public string? ADC3 { get; set; }
        public string? ADC4 { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public List<AxleDto> Axles { get; set; } = new();

        // 🔹 Calculated Property
        public double? TotalWeight { get; set; }



        // 🔥 THIS IS THE IMPORTANT PART
        public string AxlesSummary
        {
            get
            {
                if (Axles == null || Axles.Count == 0)
                    return "---";

                return string.Join(" | ", Axles.Select(a =>
                    $"Axle {a.AxleIndex}: {a.Weight:N0} kg, {a.TimeMs:F1} ms, {a.Distance:F2} m"));
            }
        }
    }
}
