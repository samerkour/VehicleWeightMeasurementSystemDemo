using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Domain.Entities
{
    public class LineEntity
    {
        public int Id { get; set; }

        public string LineCode { get; set; }
        public string LineName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<VehicleEntity> Vehicles { get; set; }
    }
}
