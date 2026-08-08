using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleWeightMeasurementSystemDemo.Reports
{
    public class VehicleReportDto
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string PlateNumber { get; set; }

        public string LineName { get; set; }

        public double? Speed { get; set; }

        public double? TotalWeight { get; set; }

        public int? AxleCount { get; set; }

        public bool Overweight { get; set; }

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
    }
}
