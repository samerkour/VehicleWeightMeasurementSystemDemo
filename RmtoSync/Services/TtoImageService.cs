using Microsoft.Extensions.Options;
using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;
using RmtoSync.Utilities;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;

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

    public byte[] BuildColorImage(CameraPhotoRecord photo, TtoPayload? payload = null, byte[]? plateImage = null)
    {
        if (!File.Exists(photo.FullPath))
            throw new FileNotFoundException("Image not found", photo.FullPath);

        payload ??= TtoPayloadFactory.Create(photo, _rahdari);

        var p2 = PlateLetterMapper.Map(photo.PlateP2?.Trim());
        var pelak = $"{photo.PlateP4}{"ایران"}{photo.PlateP3}{p2}{photo.PlateP1}";

        using var source = Image.FromFile(photo.FullPath, useEmbeddedColorManagement: true);
        using var resized = new Bitmap(source, new Size(800, 600));
        using var annotated = DrawOverlay(resized, photo, pelak, _rahdari, plateImage ?? BuildPlateImage(photo));

        var limits = ImageSizeLimits.Get(ResolveColorImageKind(payload));
        return FitJpegSize(annotated, limits.MinKb!.Value, limits.MaxKb, startQuality: 75);
    }

    public byte[] BuildPlateImage(CameraPhotoRecord photo)
    {
        if (!string.IsNullOrWhiteSpace(photo.PlateFullPath) && File.Exists(photo.PlateFullPath))
            return EncodeSavedPlateImage(photo.PlateFullPath);

        return BuildPlateImageFromOverview(photo);
    }

    /// <summary>Re-encodes the ANPR-saved plate crop to the ITS plate image limits (1–50 KB).</summary>
    private static byte[] EncodeSavedPlateImage(string plateFullPath)
    {
        using var source = Image.FromFile(plateFullPath, useEmbeddedColorManagement: true);
        var limits = ImageSizeLimits.Get(ItsImageKind.Plate);
        return FitJpegSize(source, limits.MinKb!.Value, limits.MaxKb, startQuality: 85);
    }

    /// <summary>Legacy fallback: crops the plate region out of the overview image.</summary>
    private byte[] BuildPlateImageFromOverview(CameraPhotoRecord photo)
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

    private static Bitmap DrawOverlay(Image image, CameraPhotoRecord photo, string pelak, RahdariOptions cfg, byte[] plateImage)
    {
        const int width = 751;
        const int height = 1280;
        const int headerHeight = 160;
        const int footerHeight = 90;

        var bitmap = new Bitmap(width, height);

        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            g.Clear(Color.White);

            int imageY = headerHeight;
            int imageHeight = height - headerHeight - footerHeight;
            g.DrawImage(image, new Rectangle(0, imageY, width, imageHeight));

            using (var shadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
            {
                g.FillRectangle(shadow, 0, headerHeight, width, 3);
                g.FillRectangle(shadow, 0, height - footerHeight - 3, width, 3);
            }

            using (var barBrush = new SolidBrush(Color.FromArgb(245, 255, 255, 255)))
            {
                g.FillRectangle(barBrush, 0, 0, width, headerHeight);
                g.FillRectangle(barBrush, 0, height - footerHeight, width, footerHeight);
            }

            using var borderPen = new Pen(Color.FromArgb(140, 96, 96, 96));
            g.DrawLine(borderPen, 0, headerHeight, width, headerHeight);
            g.DrawLine(borderPen, 0, height - footerHeight, width, height - footerHeight);

            using var textBrush = new SolidBrush(Color.FromArgb(235, 30, 30, 30));
            using var labelFont = CreatePersianFont(14, bold: true);
            using var valueFont = CreatePersianFont(14, bold: false);

            var pc = new PersianCalendar();
            var passDate = photo.PassDatetime;
            var instantSpeed = photo.VehicleSpeed ?? photo.AverageSpeed ?? 0;

            var fields = new (string Label, string Value)[]
            {
                ("کد پلیس", cfg.Reserved7),
                ("شماره خط عبور", photo.LineNumber.ToString(CultureInfo.InvariantCulture)),
                ("تاریخ", $"{pc.GetYear(passDate):0000}/{pc.GetMonth(passDate):00}/{pc.GetDayOfMonth(passDate):00}"),
                ("ساعت", $"{passDate:HH:mm:ss}"),
                ("کد ایستگاه", cfg.SystemCode.ToString(CultureInfo.InvariantCulture)),
                ("نام محور", cfg.StationLabel),
                ("سرعت مجاز سبک/سنگین", $"{cfg.LightVehicleSpeedViolationThresholdKmh}/{cfg.HeavyVehicleSpeedViolationThresholdKmh} km/h"),
                ("سرعت لحظه‌ای", $"{instantSpeed} km/h"),
                ("پلاک", pelak)
            };

            const float margin = 12f;
            float colWidth = (width - margin * 2) / 3f;

            for (int i = 0; i < fields.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                float x = width - margin - (col + 1) * colWidth;
                var cellRect = new RectangleF(x, 10 + row * 50, colWidth, 46);
                DrawField(g, fields[i].Label, fields[i].Value, cellRect, labelFont, valueFont, textBrush);
            }

            bool speeding = instantSpeed >= cfg.LightVehicleSpeedViolationThresholdKmh;
            using var speedFont = CreatePersianFont(16, bold: true);
            using var speedBrush = speeding
                ? new SolidBrush(Color.Firebrick)
                : new SolidBrush(Color.FromArgb(235, 30, 30, 30));

            var sfCenter = new StringFormat(StringFormatFlags.DirectionRightToLeft)
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var speedRect = new RectangleF(0, height - footerHeight, width, footerHeight);
            if (plateImage.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(plateImage);
                    using var plateImg = Image.FromStream(ms);
                    const float pad = 12f;
                    float maxPlateHeight = footerHeight - 16f;
                    float maxPlateWidth = Math.Min(width * 0.45f, 340f);
                    float scale = Math.Min(
                        maxPlateWidth / plateImg.Width,
                        maxPlateHeight / plateImg.Height);
                    int pw = Math.Max(1, (int)(plateImg.Width * scale));
                    int ph = Math.Max(1, (int)(plateImg.Height * scale));
                    int py = height - footerHeight + (footerHeight - ph) / 2;

                    g.DrawImage(plateImg, pad, py, pw, ph);

                    speedRect = new RectangleF(
                        pad + pw + 10,
                        height - footerHeight,
                        width - (pad + pw + 10) - pad,
                        footerHeight);
                }
                catch (ArgumentException)
                {
                }
            }

            g.DrawString(
                $"سرعت لحظه‌ای: {instantSpeed} km/h",
                speedFont,
                speedBrush,
                speedRect,
                sfCenter);
        }

        return bitmap;
    }

    private static void DrawField(
        Graphics g,
        string label,
        string value,
        RectangleF rect,
        Font labelFont,
        Font valueFont,
        Brush brush)
    {
        var sf = new StringFormat(StringFormatFlags.DirectionRightToLeft)
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center
        };

        string labelText = $"{label}: ";
        var labelSize = g.MeasureString(labelText, labelFont);
        var labelRect = new RectangleF(rect.Right - labelSize.Width, rect.Y, labelSize.Width, rect.Height);
        g.DrawString(labelText, labelFont, brush, labelRect, sf);

        if (value.Length == 0)
            return;

        var valueSize = g.MeasureString(value, valueFont);
        var valueRect = new RectangleF(labelRect.X - valueSize.Width + 4, rect.Y, valueSize.Width, rect.Height);
        g.DrawString(value, valueFont, brush, valueRect, sf);
    }

    private static Font CreatePersianFont(float size, bool bold)
    {
        var style = bold ? FontStyle.Bold : FontStyle.Regular;
        foreach (var name in new[] { "Vazirmatn", "Vazir", "IRANSansX", "IRANSans", "B Nazanin", "Tahoma" })
        {
            try
            {
                return new Font(new FontFamily(name), size, style);
            }
            catch (ArgumentException)
            {
            }
        }

        return new Font(FontFamily.GenericSansSerif, size, style);
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
