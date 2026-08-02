using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;

namespace VehicleWeightMeasurementSystemDemo.Domain.Services
{
    public static class DistanceCalculator
    {
        public static void CalculateDistances(VehicleDto vehicle)
        {
            var speedMs = (vehicle.Speed ?? 0) / 3.6;

            foreach (var axle in vehicle.Axles)
            {
                axle.Distance = Math.Round(speedMs * ((axle.TimeMs ?? 0) / 1000), 2);
            }
        }
    }
}
