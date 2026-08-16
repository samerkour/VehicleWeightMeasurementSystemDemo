using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Domain.Weighing
{
    public class VehicleDto
    {
        public int Id { get; set; }

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

        /// <summary>
        /// مهر زمانی monotonic لحظه‌ی رسیدن رکورد از پورت سریال.
        /// مبنای انطباق با عکس است. DateTime.Now برای این کار مناسب نیست
        /// چون با تغییر ساعت سیستم یا NTP می‌تواند به عقب بپرد.
        /// </summary>
        public long ReceivedAtTicks { get; set; }

        public List<AxleDto> Axles { get; set; } = new();

        // 🔹 Calculated Property
        public double? TotalWeight { get; set; }


        // 🔥 NEW COLUMNS
        public double? AxleWeight1 { get; set; }
        public double? AxleWeight2 { get; set; }
        public double? AxleWeight3 { get; set; }
        public double? AxleWeight4 { get; set; }
        public double? AxleWeight5 { get; set; }
        public double? AxleWeight6 { get; set; }


        public double? Axle12 { get; set; }
        public double? Axle23 { get; set; }
        public double? Axle34 { get; set; }
        public double? Axle45 { get; set; }
        public double? Axle56 { get; set; }


        // 🔥 Fields saved to [Vehicles] table (per comprehensive-system web service)
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
