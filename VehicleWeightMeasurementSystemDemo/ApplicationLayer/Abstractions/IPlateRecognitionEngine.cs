using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras.Interop.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions
{
    public interface IPlateRecognitionEngine
    {
        int Recognize(byte cameraIndex, string imagePath, string result, ref float confidence, ref RECT rc);
        string GetPlate(byte cameraIndex, int plateIndex);
    }
}
