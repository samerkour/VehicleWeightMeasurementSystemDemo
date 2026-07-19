using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Services
{
    public class SerialPortService
    {
        private readonly SerialPortSettings _settings;
        private SerialPort _port;

        public event Action<string> OnDataReceived;
        public event Action<bool> OnConnectionChanged;

        public SerialPortService(SerialPortSettings? options)
        {
            _settings = options;
        }

        public void Start()
        {
            try
            {
                _port = new SerialPort(
                     _settings.PortName,
                     _settings.BaudRate,
                     Enum.Parse<Parity>(_settings.Parity),
                     _settings.DataBits,
                     Enum.Parse<StopBits>(_settings.StopBits)
                );

                _port.DataReceived += (s, e) =>
                {
                    try
                    {
                        string data = _port.ReadLine();
                        OnDataReceived?.Invoke(data);
                    }
                    catch
                    {
                        // ignore bad reads
                    }
                };

                _port.Open();

                OnConnectionChanged?.Invoke(true); // ✅ Connected
            }
            catch
            {
                OnConnectionChanged?.Invoke(false); // ❌ Failed
            }
        }

        public void Stop()
        {
            try
            {
                if (_port != null && _port.IsOpen)
                {
                    _port.Close();
                    OnConnectionChanged?.Invoke(false);
                }
            }
            catch
            {
                OnConnectionChanged?.Invoke(false);
            }
        }
    }
}
