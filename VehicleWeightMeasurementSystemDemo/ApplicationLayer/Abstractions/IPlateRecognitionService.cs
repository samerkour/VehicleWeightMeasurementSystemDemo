using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;

namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions
{
    public interface IPlateRecognitionService
    {
        bool AddCamera(PictureBox preview);
        PlateResultDto Extract(string imagePath, byte camIndex = 0);
    }
}
