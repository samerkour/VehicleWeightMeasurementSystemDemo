using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Domain.Entities
{
    public class CameraPhotosEntity
    {
        public long Id { get; set; }

        public int? VehicleId { get; set; }
        public VehicleEntity? Vehicle { get; set; }

        public string? FileName { get; set; }
        public string? RelativePath { get; set; }
        public string? FullPath { get; set; }

        public long? FileSizeBytes { get; set; }
        public string? FileHash { get; set; }

        public DateTime? CapturedAt { get; set; }
        public DateTime? ImportedAt { get; set; }

        public string PlateP1 { get; set; }
        public string PlateP2 { get; set; }
        public string PlateP3 { get; set; }
        public string PlateP4 { get; set; }

        public double? PlateConfidence { get; set; }
        public int? PlateReadStatus { get; set; }
        public DateTime? PlateReadAt { get; set; }

        public int? PlateBoxLeft { get; set; }
        public int? PlateBoxTop { get; set; }
        public int? PlateBoxWidth { get; set; }
        public int? PlateBoxHeight { get; set; }


        public bool TerminalSent { get; set; }
        public DateTime? TerminalSentAt { get; set; }

        public bool TerminalTtoRegistered { get; internal set; }
        public bool TerminalImageExpired { get; internal set; }

        public string TerminalLastError { get; set; }
        public int? SpeedType { get; set; }

        public bool? Allowed { get; set; }
        public bool? WrongDirection { get; set; }
        public double? FirstToLastAxlesLen { get; set; }
   
    }
}
