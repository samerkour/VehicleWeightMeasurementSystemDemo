using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Camera;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static VehicleWeightMeasurementSystemDemo.Services.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo.Services
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


        public CameraDto AddCamera(PictureBox preview)
        {
            if (_cameras.Any())
                throw new InvalidOperationException("Camera already added.");

            byte camNum = 0;

            var cam = new SATPA(
                camNum,
                $"cam{camNum}",
                preview,
                License.per_camera
            );

            _cameras.Add(cam);

            var dto = new CameraDto
            {
                CameraNumber = 1,//cam.name,
                Name = cam.name
            };

            //OnCameraAdded?.Invoke(dto);

            return dto;
        }

        public PlateRecognitionService(IPlateRecognitionEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public PlateResultDto Extract(string imagePath, byte camIndex = 0)
        {
            if (string.IsNullOrEmpty(imagePath))
                return new PlateResultDto() ;


            var camera = _cameras[camIndex];
            //camera.save_setting();

            var results = new PlateResultDto();

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

            for (int i = 0; i < count; i++)
            {
                string json = _engine.GetPlate(camIndex, i);

                var plate = JsonSerializer.Deserialize<SPlateResult>(json);

                //var image = (Bitmap)camera.make_pic_plate(
                //    plate.plate_image_pointer,
                //    plate.plate_height,
                //    plate.plate_width
                //);

                results = new PlateResultDto
                    {
                        //PlateNumber = plate.plate_string,
                        PlateNumber = FormatIranianPlate(plate.plate_string_unicode_indices),
                        Confidence = plate.confidence,
                        Country = countries[plate.country]
                        //,
                        //PlateImage = image
                    };
            }

            return results;
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

      

        //public string FormatIranianPlate(string plate)
        //{
        //    if (string.IsNullOrWhiteSpace(plate) || plate.Length < 7)
        //        return plate;

        //    // Example: ۱۸س۲۴۴۱۱
        //    var part1 = plate.Substring(0, 2);   // ۱۸
        //    var letter = plate.Substring(2, 1);  // س
        //    var part2 = plate.Substring(3, 3);   // ۲۴۴
        //    var part3 = plate.Substring(6);      // ۱۱

        //    return $"{part1} {letter} {part2} {part3}";
        //}
    }
}
