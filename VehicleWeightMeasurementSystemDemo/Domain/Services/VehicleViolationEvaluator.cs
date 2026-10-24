using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;

namespace VehicleWeightMeasurementSystemDemo.Domain.Services
{
    /// <summary>
    /// Single entry point for one pass: calibrate the raw WIM weight, classify the
    /// vehicle, apply the class limits, and produce the violation codes.
    ///
    /// The calibration is applied exactly once, here, so the class, the limits, the
    /// overweight value, <c>Vehicles.CrimeCodes</c>, and the stored
    /// <c>Vehicles.TotalWeight</c> can never disagree.
    /// </summary>
    public static class VehicleViolationEvaluator
    {
        public static VehicleViolationAssessment Evaluate(
            VehicleDto vehicle,
            VehicleClassificationSettings settings,
            IVehicleComplianceService compliance,
            decimal weightAlpha,
            bool plateRead)
        {
            if (vehicle is null) throw new ArgumentNullException(nameof(vehicle));
            if (compliance is null) throw new ArgumentNullException(nameof(compliance));

            var effectiveWeightKg = VehicleWeightCalibration.Apply(vehicle.TotalWeight, weightAlpha);

            vehicle.VehicleClass = VehicleClassifier.Classify(vehicle, settings, effectiveWeightKg);

            var assessment = compliance.Assess(vehicle.VehicleClass, vehicle.Speed, effectiveWeightKg);

            vehicle.TotalOverWeight = assessment.TotalOverWeight;
            vehicle.MaxAllowedWeightForClass = assessment.MaxAllowedWeightForClass;
            vehicle.MaxAllowedSpeedForClass = assessment.MaxAllowedSpeedForClass;
            vehicle.CrimeCodes = VehicleComplianceService.FormatCrimeCodes(assessment.CrimeCodes);

            // A pass is "allowed" only when the plate was read AND the class limits
            // are respected. The recovery script applies the same rule.
            vehicle.Allowed = plateRead && assessment.IsCompliant;

            return assessment;
        }
    }
}
