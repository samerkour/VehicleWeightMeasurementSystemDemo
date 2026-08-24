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

        public string PlateP1 { get; set; } = string.Empty;
        public string PlateP2 { get; set; } = string.Empty;
        public string PlateP3 { get; set; } = string.Empty;
        public string PlateP4 { get; set; } = string.Empty;

        public double? PlateConfidence { get; set; }
        public int? PlateReadStatus { get; set; }
        public DateTime? PlateReadAt { get; set; }

        // 🔥 تصویر کراپ‌شده پلاک (C:\Temp\RahdariImages)
        public string? PlateFileName { get; set; }
        public string? PlateRelativePath { get; set; }
        public string? PlateFullPath { get; set; }

        public int? PlateBoxLeft { get; set; }
        public int? PlateBoxTop { get; set; }
        public int? PlateBoxWidth { get; set; }
        public int? PlateBoxHeight { get; set; }


        public bool TerminalSent { get; set; }
        public DateTime? TerminalSentAt { get; set; }

        public bool TerminalTtoRegistered { get; internal set; }
        public bool TerminalImageExpired { get; internal set; }

        public string? TerminalLastError { get; set; }
        public int? SpeedType { get; set; }

        public bool? Allowed { get; set; }
        public bool? WrongDirection { get; set; }
        public double? FirstToLastAxlesLen { get; set; }

        public DateTime? TerminalImageExpiredAt { get; set; }
        public DateTime? TerminalTtoRegisteredAt { get; set; }
        public long? TerminalPassInfoId { get; set; }
        public long? TerminalPackId { get; set; }
        public DateTime? TerminalImageDeadlineAt { get; set; }

        public int? CarClass13 { get; set; }
        public string? CrimeCodes { get; set; }

        public decimal? OcrScore { get; set; }

        public int? HeadGap { get; set; }
        public int? Gap { get; set; }

        public int? LengthAxles12 { get; set; }
        public int? LengthAxles23 { get; set; }
        public int? LengthAxles34 { get; set; }
        public int? LengthAxles45 { get; set; }
        public int? LengthAxles56 { get; set; }
        public int? LengthAxles67 { get; set; }
        public int? LengthAxles78 { get; set; }
        public int? LengthAxlesMoreThan8 { get; set; }

        public int? TotalWeightA { get; set; }
        public int? TotalWeightB { get; set; }
        public int? TotalWeightC { get; set; }
        public int? TotalOverWeight { get; set; }

        public long? PassInfoId { get; set; }
        public long? PreviousDeviceCode { get; set; }
    }
}
