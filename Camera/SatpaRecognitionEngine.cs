using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Services;
using static VehicleWeightMeasurementSystemDemo.Services.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo.Camera
{
    public class SatpaRecognitionEngine : IPlateRecognitionEngine
    {
        public int Recognize(byte cameraIndex, string imagePath, string result, ref float confidence, ref RECT rc)
        {
            return satpa_recognize(cameraIndex, imagePath, result, ref confidence, ref rc);
        }

        public string GetPlate(byte cameraIndex, int plateIndex)
        {
            string buffer = new string(' ', 2000);
            satpa_get_plate(cameraIndex, plateIndex, buffer);
            return buffer.ToString();
        }
    }
}
