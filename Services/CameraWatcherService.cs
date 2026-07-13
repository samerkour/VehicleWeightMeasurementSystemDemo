using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleWeightMeasurementSystemDemo.Services
{
    using Microsoft.Extensions.Options;

    public class CameraWatcherService
    {
        private readonly CameraSettings _settings;
        private FileSystemWatcher _watcher;

        public event Action<string> OnImageCaptured;
        public event Action<bool> OnStatusChanged; // 🔥 for UI

        public CameraWatcherService(CameraSettings? options)
        {
            _settings = options;
        }

        public void Start()
        {
            try
            {
                if (!Directory.Exists(_settings.FolderPath))
                {
                    OnStatusChanged?.Invoke(false);
                    return;
                }

                _watcher = new FileSystemWatcher(_settings.FolderPath, _settings.Filter)
                {
                    IncludeSubdirectories = _settings.IncludeSubfolders,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
                    InternalBufferSize = 64 * 1024
                };

                _watcher.Created += async (s, e) =>
                {
                    if (await WaitForFileReady(e.FullPath))
                    {
                        OnImageCaptured?.Invoke(e.FullPath);
                    }
                };

                OnStatusChanged?.Invoke(true);
            }
            catch
            {
                OnStatusChanged?.Invoke(false);
            }
        }


        public void Stop()
        {
            try
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Dispose();
                    OnStatusChanged?.Invoke(false);
                }
            }
            catch
            {
                OnStatusChanged?.Invoke(false);
            }
        }

        private async Task<bool> WaitForFileReady(string path)
        {
            const int maxRetries = 10;
            const int delayMs = 200;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (stream.Length > 0)
                            return true;
                    }
                }
                catch
                {
                    // file still locked
                }

                await Task.Delay(delayMs);
            }

            return false;
        }
    }

}
