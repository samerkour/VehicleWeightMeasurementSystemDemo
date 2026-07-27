using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace VehicleWeightMeasurementSystemDemo.Services
{

    using Microsoft.Extensions.Options;

    public class CameraWatcherService
    {
        private readonly SnapshotCameraSettings _settings;
        private readonly List<LineWatcher> _lines = new();

        public event Action<int, string> OnImageCaptured;
        public event Action<bool> OnStatusChanged;

        // ✅ DI-friendly constructor
        public CameraWatcherService(IOptions<SnapshotCameraSettings> options)
        {
            _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public void Start(List<int> lineIds)
        {
            Stop(); // prevent duplicates

            try
            {
                foreach (var lineId in lineIds)
                {
                    var folder = Path.Combine(_settings.WatchRootPath, $"Line{lineId}");

                    if (!Directory.Exists(folder))
                        continue;

                    var watcher = new FileSystemWatcher(folder, _settings.Filter)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = _settings.IncludeSubfolders,
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
                        InternalBufferSize = 64 * 1024
                    };

                    int currentLineId = lineId;

                    watcher.Created += async (s, e) =>
                    {
                        if (await WaitForFileReady(e.FullPath))
                        {
                            OnImageCaptured?.Invoke(currentLineId, e.FullPath);
                        }
                    };

                    _lines.Add(new LineWatcher
                    {
                        LineId = lineId,
                        FolderPath = folder,
                        Watcher = watcher
                    });
                }

                OnStatusChanged?.Invoke(_lines.Any());
            }
            catch
            {
                OnStatusChanged?.Invoke(false);
            }
        }

        public void Stop()
        {
            foreach (var line in _lines)
            {
                try
                {
                    line.Watcher.EnableRaisingEvents = false;
                    line.Watcher.Dispose();
                }
                catch { }
            }

            _lines.Clear();
            OnStatusChanged?.Invoke(false);
        }

        private async Task<bool> WaitForFileReady(string path)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    if (stream.Length > 0)
                        return true;
                }
                catch { }

                await Task.Delay(200);
            }

            return false;
        }
    }

}
