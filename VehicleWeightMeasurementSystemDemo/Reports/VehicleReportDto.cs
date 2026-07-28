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
    }
}
