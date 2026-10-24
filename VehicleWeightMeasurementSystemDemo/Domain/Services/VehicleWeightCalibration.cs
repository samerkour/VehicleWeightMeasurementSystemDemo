using System;

namespace VehicleWeightMeasurementSystemDemo.Domain.Services
{
    /// <summary>
    /// Applies <c>WeightSettings:Alpha</c> exactly once and with the same rounding
    /// that is used when the calibrated weight is written to <c>Vehicles.TotalWeight</c>.
    ///
    /// Classification, violation assessment, and persistence must all use this value;
    /// otherwise the stored weight and the stored crime codes describe two different
    /// vehicles.
    /// </summary>
    public static class VehicleWeightCalibration
    {
        /// <summary>
        /// Converts a raw WIM weight (kg) to the calibrated weight stored in
        /// <c>Vehicles.TotalWeight</c>. <c>SaveAsync</c> and the violation workflow
        /// both call this method.
        /// </summary>
        public static double? Apply(double? rawWeightKg, decimal alpha)
        {
            if (rawWeightKg is not { } raw)
                return null;

            return (int)Math.Round((decimal)raw * alpha);
        }
    }
}
