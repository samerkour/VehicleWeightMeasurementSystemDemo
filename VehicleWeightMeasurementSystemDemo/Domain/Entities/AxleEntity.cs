using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Domain.Entities
{
    public class AxleEntity
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public VehicleEntity Vehicle { get; set; }

        public int AxleIndex { get; set; }

        public double Weight { get; set; }

        public double? TimeMs { get; set; }
        public double? Distance { get; set; }

        public double? LengthToNext { get; set; }

        public bool? IsOverweight { get; set; }
    }
}
