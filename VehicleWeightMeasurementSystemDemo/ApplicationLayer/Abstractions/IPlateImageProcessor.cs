using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using static VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras.Interop.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions
{
    public interface IPlateImageProcessor
    {
        Bitmap CropPlate(string imagePath);
        Bitmap CropPlate(string imagePath, RECT rect);
    }
}
