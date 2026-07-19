using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo
{
    public class LineWatcher
    {
        public int LineId { get; set; }
        public string FolderPath { get; set; }
        public FileSystemWatcher Watcher { get; set; }
    }
}
