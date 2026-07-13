using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VehicleWeightMeasurementSystemDemo.Services.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo.Services
{
    public interface IPlateRecognitionEngine
    {
        int Recognize(byte cameraIndex, string imagePath, string result, ref float confidence, ref RECT rc);
        string GetPlate(byte cameraIndex, int plateIndex);
    }
}
