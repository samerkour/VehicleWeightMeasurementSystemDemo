using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;
using RmtoSync.Utilities;
using Microsoft.Extensions.Options;

namespace RmtoSync.Services;

public sealed class TtoImageService
{
    private readonly RahdariOptions _rahdari;
    private readonly RmtoSyncOptions _sync;

    public TtoImageService(IOptions<RahdariOptions> rahdari, IOptions<RmtoSyncOptions> sync)
    {
        _rahdari = rahdari.Value;
        _sync = sync.Value;
    }

    public byte[] BuildColorImage(CameraPhotoRecord photo, TtoPayload? payload = null)
    {
        if (!File.Exists(photo.FullPath))
            throw new FileNotFoundException("Image not found", photo.FullPath);

        payload ??= TtoPayloadFactory.Create(photo, _rahdari);

        var p2 = PlateLetterMapper.Map(photo.PlateP2?.Trim());
        var pelak = $"{photo.PlateP4}{"ایران"}{photo.PlateP3}{p2}{photo.PlateP1}";
        var passDate = photo.PassDatetime;
        var isDay = passDate.Hour is >= 6 and <= 18;

        using var source = Image.FromFile(photo.FullPath, useEmbeddedColorManagement: true);
        using var resized = new Bitmap(source, new Size(800, 600));
        using var annotated = DrawOverlay(resized, pelak, photo.LineNumber.ToString(), _rahdari.StationLabel, passDate, isDay);

        var limits = ImageSizeLimits.Get(ResolveColorImageKind(payload));
        return FitJpegSize(annotated, limits.MinKb!.Value, limits.MaxKb, startQuality: 75);
    }

    public byte[] BuildPlateImage(CameraPhotoRecord photo)
    {
        if (!File.Exists(photo.FullPath))
            return Array.Empty<byte>();

        var left = photo.PlateBoxLeft ?? 0;
        var top = photo.PlateBoxTop ?? 0;
        var width = photo.PlateBoxWidth ?? 0;
        var height = photo.PlateBoxHeight ?? 0;

        using var source = Image.FromFile(photo.FullPath, useEmbeddedColorManagement: true);

        Bitmap plateBitmap;
        if (width > 0 && height > 0)
        {
            var rect = ClampRect(
                new Rectangle(left, top, Math.Max(1, width), Math.Max(1, height)),
                source.Width,
                source.Height);
            plateBitmap = new Bitmap(rect.Width, rect.Height);
            using (var g = Graphics.FromImage(plateBitmap))
                g.DrawImage(source, 0, 0, rect, GraphicsUnit.Pixel);
        }
        else if (_sync.UsePlateCropFallback)
        {
            plateBitmap = BuildPlateFallbackFromScene(source);
        }
        else
        {
            return Array.Empty<byte>();
        }

        using (plateBitmap)
        {
            var limits = ImageSizeLimits.Get(ItsImageKind.Plate);
            return FitJpegSize(plateBitmap, limits.MinKb!.Value, limits.MaxKb, startQuality: 85);
        }
    }

    /// <summary>Lower-center crop when SATPA bbox is missing (ITS plate image 1–50 KB).</summary>
    private static Bitmap BuildPlateFallbackFromScene(Image source)
    {
        var cropW = Math.Max(120, source.Width * 2 / 5);
        var cropH = Math.Max(40, source.Height / 5);
        var x = (source.Width - cropW) / 2;
        var y = Math.Max(0, source.Height * 3 / 5 - cropH / 2);
        var rect = ClampRect(new Rectangle(x, y, cropW, cropH), source.Width, source.Height);

        var cropped = new Bitmap(rect.Width, rect.Height);
        using (var g = Graphics.FromImage(cropped))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, rect, GraphicsUnit.Pixel);
        }

        if (cropped.Width < 200)
        {
            var scale = Math.Max(2, 200 / Math.Max(1, cropped.Width));
            var upscaled = new Bitmap(cropped, cropped.Width * scale, cropped.Height * scale);
            cropped.Dispose();
            return upscaled;
        }

        return cropped;
    }

    private static ItsImageKind ResolveColorImageKind(TtoPayload payload)
    {
        if (payload.IsHeavy && payload.TotalWeight > 0)
            return ItsImageKind.MainWim;

        if (payload.SpeedType is TtoFieldValues.SpeedType.Average or TtoFieldValues.SpeedType.InstantAndAverage)
            return ItsImageKind.MainAverageSpeed;

        return ItsImageKind.Main;
    }

    private static Rectangle ClampRect(Rectangle rect, int maxW, int maxH)
    {
        var x = Math.Clamp(rect.X, 0, Math.Max(0, maxW - 1));
        var y = Math.Clamp(rect.Y, 0, Math.Max(0, maxH - 1));
        var w = Math.Clamp(rect.Width, 1, maxW - x);
        var h = Math.Clamp(rect.Height, 1, maxH - y);
        return new Rectangle(x, y, w, h);
    }

    private static Bitmap DrawOverlay(
        Image image, string pelak, string line, string station,
        DateTime passDate, bool isDay)
    {
        var bitmap = new Bitmap(image);
        var brush = isDay ? Brushes.DarkBlue : Brushes.Aqua;
        var pc = new PersianCalendar();
        var dateText = $"{passDate:HH:mm:ss:fff} {pc.GetYear(passDate):0000}/{pc.GetMonth(passDate):00}/{pc.GetDayOfMonth(passDate):00}";

        using var g = Graphics.FromImage(bitmap);
        using var font = new Font("Tahoma", 14, FontStyle.Bold);
        g.DrawString("پلاک:" + pelak, font, brush, 10f, 520f);
        g.DrawString("لاین:" + line, font, brush, 250f, 520f);
        g.DrawString("ایستگاه:" + station, font, brush, 350f, 520f);
        g.DrawString("تاریخ و ساعت:" + dateText, font, brush, 10f, 560f);
        g.DrawString("نام شرکت:شرکت فراسو", font, brush, 350f, 560f);
        return bitmap;
    }

    private static byte[] FitJpegSize(Image image, int minKb, int maxKb, long startQuality)
    {
        var minBytes = minKb * 1024;
        var maxBytes = maxKb * 1024;
        var quality = startQuality;
        byte[]? best = null;

        while (quality >= 20)
        {
            var bytes = ToJpegBytes(image, quality);
            best = bytes;

            if (bytes.Length <= maxBytes)
            {
                if (bytes.Length >= minBytes)
                    return bytes;

                break;
            }

            quality -= 10;
        }

        if (best != null && best.Length <= maxBytes)
            return best;

        using var scaled = new Bitmap(image, new Size(Math.Max(1, image.Width * 3 / 4), Math.Max(1, image.Height * 3 / 4)));
        return FitJpegSize(scaled, minKb, maxKb, startQuality: Math.Min(startQuality, 70));
    }

    private static byte[] ToJpegBytes(Image image, long quality)
    {
        using var ms = new MemoryStream();
        var codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");
        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        image.Save(ms, codec, parameters);
        return ms.ToArray();
    }
}
