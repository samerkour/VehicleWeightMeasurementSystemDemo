using System;
using System.Linq;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;

namespace VehicleWeightMeasurementSystemDemo.Domain.Services
{
    /// <summary>
    /// تعیین کلاس خودرو (CarClass13) بر اساس جدول طبقه‌بندی ۱۴گانه وب‌سرویس سامانه جامع:
    /// 1=سواری، 2=وانت، 3=کامیون ۲محور سبک، 4=کامیون ۲محور سنگین، 5=اتوبوس ۲محور، 6=اتوبوس ۳محور،
    /// 7=کامیون ۳محور، 8=تریلر ۳محور، 9=کامیون ۴محور، 10=تریلر ۴محور ۱۲چرخ، 11=تریلر ۴محور ۱۴چرخ،
    /// 12=تریلر ۵محور ۱۲چرخ، 13=تریلر ۵محور ۱۸چرخ، 14=تریلر ۶محور
    /// </summary>
    public static class VehicleClassifier
    {
        public static int? Classify(VehicleDto vehicle, VehicleClassificationSettings settings)
        {
            if (vehicle == null || settings == null)
                return null;

            var axleCount = vehicle.AxleCount ?? vehicle.Axles.Count;
            var weight = vehicle.TotalWeight ?? 0;

            return axleCount switch
            {
                1 => weight <= settings.PickupMaxWeight ? 1 : 2,
                2 => weight switch
                {
                    var w when w <= settings.SedanMaxWeight => 1,
                    var w when w <= settings.PickupMaxWeight => 2,
                    var w when w <= settings.LightTruckMaxWeight => 3,
                    _ => 4
                },
                3 => weight <= settings.Truck3MaxWeight ? 7 : 8,
                4 => weight <= settings.Truck4MaxWeight ? 9 : 10,
                5 => 12,
                _ => 14
            };
        }

        /// <summary>
        /// مجموع فاصله بین محور اول تا آخرین محور (طول خودرو بر حسب متر).
        /// </summary>
        public static double? ComputeVehicleLength(VehicleDto vehicle)
        {
            if (vehicle?.Axles == null || vehicle.Axles.Count == 0)
                return null;

            var distances = vehicle.Axles
                .Where(a => a.Distance.HasValue)
                .Select(a => a.Distance.Value)
                .ToList();

            if (distances.Count == 0)
                return null;

            return Math.Round(distances.Max(), 2);
        }
    }
}