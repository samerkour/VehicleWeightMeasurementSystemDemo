using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Models;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace VehicleWeightMeasurementSystemDemo.Services
{
    public class VehicleParser
    {
        public static VehicleDto Parse(string raw)
        {
            var parts = raw.Split(',');

            var axles = ParseAxles(parts); // ✅ parse ONCE

            return new VehicleDto
            {
                ADC1 = parts[0],
                ADC2 = parts[1],
                Speed = double.Parse(parts[3]) / 10,
                LineId = int.Parse(parts[4]),
                AxleCount = int.Parse(parts[5]),
                ADC3 = parts[18],
                ADC4 = parts[19],

                Axles = axles,

                // ✅ Correct TotalWeight
                TotalWeight = axles.Sum(a => a.Weight)
            };
        }

        private static List<AxleDto> ParseAxles(string[] parts)
        {
            var list = new List<AxleDto>();

            int index = 7;
            for (int i = 0; i < 6; i++)
            {
                double weight = double.Parse(parts[index++]);
                double time = double.Parse(parts[index++]) / 10;

                list.Add(new AxleDto
                {
                    Index = i + 1,
                    Weight = weight,
                    TimeMs = time
                });
            }

            return list;
        }
    }
}
