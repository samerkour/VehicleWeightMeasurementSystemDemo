using System;
using System.Threading.Tasks;
using Serilog;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions;
using Windows.Devices.Geolocation;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure
{
    /// <summary>
    /// دریافت موقعیت از Windows.Devices.Geolocation (Geolocator).
    /// در صورت در دسترس نبودن GPS، null برمی‌گرداند (بدون crash).
    /// </summary>
    public class WindowsGeoLocationService : IGeoLocationService
    {
        public async Task<GeoPosition?> GetPositionAsync()
        {
            try
            {
                var geolocator = new Geolocator
                {
                    DesiredAccuracy = PositionAccuracy.High
                };

                var access = await Geolocator.RequestAccessAsync();
                if (access != GeolocationAccessStatus.Allowed)
                {
                    Log.Warning("GeoLocation access denied: {Access}", access);
                    return null;
                }

                var position = await geolocator.GetGeopositionAsync(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15));

                if (position?.Coordinate == null)
                    return null;

                return new GeoPosition
                {
                    Latitude = position.Coordinate.Latitude,
                    Longitude = position.Coordinate.Longitude
                };
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get device location");
                return null;
            }
        }
    }
}