using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo
{
    public class SerialPortSettings
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public string Parity { get; set; }
        public int DataBits { get; set; }
        public string StopBits { get; set; }
    }
}
