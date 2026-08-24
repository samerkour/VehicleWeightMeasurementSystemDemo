using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras
{
    /// <summary>
    /// ذخیره تصویر کراپ‌شده پلاک در پوشه‌ی تنظیم‌شده (appsettings → PlateImageStore:RootFolder)
    /// با نام استاندارد: {PlateNumber}_{yyyyMMdd_HHmmssfff}.jpg
    /// </summary>
    public class PlateImageFileStore
    {
        private const long JpegQuality = 95L;

        private readonly string _rootFolder;

        public PlateImageFileStore(PlateImageStoreSettings? settings)
        {
            _rootFolder = string.IsNullOrWhiteSpace(settings?.RootFolder)
                ? @"C:\Temp\RahdariImages"
                : settings.RootFolder;
        }

        public sealed class PlateFileInfo
        {
            public string FileName { get; init; } = string.Empty;
            public string RelativePath { get; init; } = string.Empty;
            public string FullPath { get; init; } = string.Empty;
        }

        /// <summary>
        /// ذخیره پلاک؛ اگر تصویر/پلاک معتبر نباشد null برمی‌گردد.
        /// </summary>
        public PlateFileInfo? Save(Bitmap? plateImage, string? plateNumber)
        {
            if (plateImage == null || plateImage.Width <= 0 || plateImage.Height <= 0)
                return null;

            try
            {
                Directory.CreateDirectory(_rootFolder);
                var fileName = $"{plateNumber}_{DateTime.Now:yyyyMMdd_HHmmssfff}.jpg";
                var fullPath = Path.Combine(_rootFolder, fileName);

                SaveHighQualityJpeg(plateImage, fullPath);

                return new PlateFileInfo
                {
                    FileName = fileName,
                    RelativePath = $"{ToRelativeUrl(_rootFolder)}/{fileName}",
                    FullPath = fullPath
                };
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to save cropped plate image ({Plate})", plateNumber);
                return null;
            }
        }

        /// <summary>
        /// "C:\Temp\RahdariImages" → "/Temp/RahdariImages"
        /// </summary>
        private string ToRelativeUrl(string folder)
        {
            var root = Path.GetPathRoot(folder)?.TrimEnd('\\', '/') ?? string.Empty;
            var relative = folder.Substring(root.Length)
                                 .TrimStart('\\', '/')
                                 .Replace('\\', '/');

            return $"/{relative}";
        }

        private void SaveHighQualityJpeg(Bitmap image, string fullPath)
        {
            var jpegCodec = ImageCodecInfo.GetImageEncoders()
                .First(c => c.FormatID == ImageFormat.Jpeg.Guid);

            using var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, JpegQuality);

            image.Save(fullPath, jpegCodec, encoderParams);
        }
    }
}
