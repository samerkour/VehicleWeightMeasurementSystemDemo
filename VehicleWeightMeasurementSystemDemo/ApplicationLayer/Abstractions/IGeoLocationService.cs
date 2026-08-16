using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions
{
    /// <summary>
    /// تأمین مختصات جغرافیایی ایستگاه (طول و عرض جغرافیایی) از GPS/Windows Location.
    /// </summary>
    public interface IGeoLocationService
    {
        Task<GeoPosition?> GetPositionAsync();
    }

    public class GeoPosition
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}