using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Serilog;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions;
using static VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras.Interop.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras
{
    public class PlateImageProcessor : IPlateImageProcessor
    {
        private readonly IPlateRecognitionEngine _engine;

        public PlateImageProcessor(IPlateRecognitionEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public Bitmap CropPlate(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return null;

            RECT rc = new RECT();
            string buffer = new string(' ', 20);
            float confidence = 0;

            int count = _engine.Recognize(0, imagePath, buffer, ref confidence, ref rc);
            if (count <= 0)
                return null;

            return CropPlate(imagePath, rc);
        }

        public Bitmap CropPlate(string imagePath, RECT rect)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return null;

            try
            {
                using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                ms.Position = 0;

                using var image = Image.FromStream(ms);

                int left = Math.Max(0, rect.left);
                int top = Math.Max(0, rect.top);
                int width = Math.Min(image.Width - left, rect.right - rect.left);
                int height = Math.Min(image.Height - top, rect.bottom - rect.top);

                if (width <= 0 || height <= 0)
                    return null;

                var destRect = new Rectangle(0, 0, width, height);
                var srcRect = new Rectangle(left, top, width, height);

                var cropped = new Bitmap(width, height);
                using (var g = Graphics.FromImage(cropped))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
                }

                return cropped;
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to crop plate from {Path}: {Message}", imagePath, ex.Message);
                return null;
            }
        }
    }
}
