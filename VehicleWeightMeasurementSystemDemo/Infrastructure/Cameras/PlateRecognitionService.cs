using System.Text.Json;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras.Interop;
using static VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras.Interop.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras
{
    public class PlateRecognitionService
    {
        private readonly IPlateRecognitionEngine _engine;

        private readonly string[] countries =
        {
            "UNKNOWN","TURKEY","IRAN","RUSSIA","AZERBAIJAN",
            "GEORGIA","TRANZIT","AFGHANISTAN","ARMENIA","IRAQ"
        };

        private readonly List<SATPA> _cameras = new();

        public PlateRecognitionService(IPlateRecognitionEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public bool AddCamera(PictureBox preview)
        {
            if (_cameras.Any())
                throw new InvalidOperationException("Camera already added.");

            var cam = new SATPA(
                0,
                "cam0",
                preview,
                License.per_camera
            );

            _cameras.Add(cam);
            return true;
        }

        public PlateResultDto Extract(string imagePath, byte camIndex = 0)
        {
            if (string.IsNullOrEmpty(imagePath))
                return new PlateResultDto();

            RECT rc = new RECT();
            string buffer = new string(' ', 20);
            float confidence = 0;

            int count = _engine.Recognize(
                camIndex,
                imagePath,
                buffer,
                ref confidence,
                ref rc
            );

            PlateResultDto best = null;

            for (int i = 0; i < count; i++)
            {
                string json = _engine.GetPlate(camIndex, i);

                var plate = JsonSerializer.Deserialize<SPlateResult>(json);
                if (plate == null)
                    continue;

                var result = new PlateResultDto
                {
                    PlateNumber = FormatIranianPlate(plate.plate_string_unicode_indices),
                    Confidence = plate.confidence,
                    Country = countries[plate.country]
                };

                // در صورت وجود چند پلاک، مطمئن‌ترین نتیجه را برمی‌گردانیم
                if (best == null || plate.confidence > best.Confidence)
                    best = result;
            }

            return best ?? new PlateResultDto();
        }

        public static string FormatIranianPlate(int[] unicodeIndices)
        {
            if (unicodeIndices == null || unicodeIndices.Length != 8)
                return "---";

            var chars = unicodeIndices
                .Select(u => char.ConvertFromUtf32(u))
                .ToArray();

            string part1 = $"{chars[0]}{chars[1]}";           // ۱۸
            string letter = chars[2];                         // س
            string part2 = $"{chars[3]}{chars[4]}{chars[5]}"; // ۲۴۴
            string part3 = $"{chars[6]}{chars[7]}";           // ۱۱

            // RTL-safe formatting
            string rtlMark = "\u200F"; // Right-to-left mark

            return rtlMark + $"{part3} ایران {part2} {letter} {part1}";
        }
    }
}
