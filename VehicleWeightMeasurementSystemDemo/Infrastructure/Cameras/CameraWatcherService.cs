using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Serilog;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras
{
    public class CameraWatcherService : IDisposable
    {
        private const int MaxPublishedHistory = 4096;

        private readonly SnapshotCameraSettings _settings;

        private readonly List<LineWatcher> _lines = new();
        private readonly object _linesLock = new();

        // dedup با حافظه‌ی محدود: Queue ترتیب ورود را نگه می‌دارد تا قدیمی‌ترها حذف شوند
        private readonly HashSet<string> _published = new(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> _publishOrder = new();
        private readonly object _publishLock = new();

        private List<int> _activeLineIds = new();
        private int _restarting;   // Interlocked guard
        private bool _disposed;

        /// <summary>lineId, fullPath, Stopwatch.GetTimestamp()</summary>
        public event Action<int, string, long> OnImageCaptured;
        public event Action<bool> OnStatusChanged;

        public bool IsRunning
        {
            get
            {
                lock (_linesLock)
                {
                    return _lines.Any(w => w.Watcher.EnableRaisingEvents);
                }
            }
        }

        public CameraWatcherService(IOptions<SnapshotCameraSettings> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public void Start(List<int> lineIds)
        {
            if (lineIds == null) throw new ArgumentNullException(nameof(lineIds));

            // notify: false تا وضعیت «قطع» کاذب پیش از راه‌اندازی منتشر نشود
            StopInternal(notify: false);

            _activeLineIds = lineIds.ToList();

            if (!_settings.Enabled)
            {
                Log.Information("Snapshot camera watcher disabled by configuration");
                OnStatusChanged?.Invoke(false);
                return;
            }

            bool any = false;

            foreach (var lineId in _activeLineIds)
            {
                if (TryStartLine(lineId))
                    any = true;
            }

            OnStatusChanged?.Invoke(any);
        }

        private bool TryStartLine(int lineId)
        {
            var folder = Path.Combine(_settings.WatchRootPath, $"Line{lineId}");

            if (!Directory.Exists(folder))
            {
                Log.Warning("Watch folder missing for line {LineId}: {Folder}", lineId, folder);
                return false;
            }

            try
            {
                var watcher = new FileSystemWatcher(folder, _settings.Filter)
                {
                    IncludeSubdirectories = _settings.IncludeSubfolders,
                    NotifyFilter = NotifyFilters.FileName,
                    InternalBufferSize = 64 * 1024
                };

                int currentLineId = lineId;

                // بدون async: مهر زمانی همان لحظه ثبت و فوراً منتشر می‌شود.
                // بررسی آماده‌بودن فایل به مصرف‌کننده منتقل شده تا ترتیب حفظ شود.
                watcher.Created += (s, e) => Publish(currentLineId, e.FullPath);

                // بعضی دوربین‌ها با نام موقت می‌نویسند و بعد rename می‌کنند
                watcher.Renamed += (s, e) => Publish(currentLineId, e.FullPath);

                watcher.Error += (s, e) =>
                {
                    Log.Error(e.GetException(),
                        "Watcher error on line {LineId}, restarting", currentLineId);

                    // dispose کردن watcher داخل handler خودش امن نیست؛ به thread دیگری منتقل می‌شود
                    _ = Task.Run(() => RestartWatcher(currentLineId));
                };

                watcher.EnableRaisingEvents = true;

                lock (_linesLock)
                {
                    _lines.Add(new LineWatcher
                    {
                        LineId = lineId,
                        FolderPath = folder,
                        Watcher = watcher
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start watcher for line {LineId}", lineId);
                return false;
            }
        }

        private void RestartWatcher(int lineId)
        {
            // اگر چند رویداد Error پشت‌سرهم بیاید، فقط یک restart اجرا می‌شود
            if (Interlocked.CompareExchange(ref _restarting, 1, 0) != 0)
                return;

            try
            {
                if (_disposed)
                    return;

                LineWatcher existing;

                lock (_linesLock)
                {
                    existing = _lines.FirstOrDefault(l => l.LineId == lineId);
                    if (existing != null)
                        _lines.Remove(existing);
                }

                if (existing != null)
                    DisposeWatcher(existing);

                if (!TryStartLine(lineId))
                {
                    Log.Error("Restart failed for line {LineId}", lineId);
                    return;
                }

                // رویدادهای از‌دست‌رفته در فاصله‌ی سرریز buffer را جبران می‌کند
                RescanFolder(lineId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error restarting watcher for line {LineId}", lineId);
            }
            finally
            {
                Interlocked.Exchange(ref _restarting, 0);
            }
        }

        private void RescanFolder(int lineId)
        {
            string folder;

            lock (_linesLock)
            {
                folder = _lines.FirstOrDefault(l => l.LineId == lineId)?.FolderPath;
            }

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                return;

            try
            {
                var option = _settings.IncludeSubfolders
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

                var files = new DirectoryInfo(folder)
                    .GetFiles(_settings.Filter ?? "*.*", option)
                    .OrderBy(f => f.CreationTimeUtc)
                    .ToList();

                // فایل‌های منتشرشده در Publish با dedup رد می‌شوند
                foreach (var file in files)
                    Publish(lineId, file.FullName);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Rescan failed for line {LineId} folder {Folder}", lineId, folder);
            }
        }

        private void Publish(int lineId, string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return;

            // FileSystemWatcher می‌تواند برای یک فایل چند بار رویداد بدهد
            lock (_publishLock)
            {
                if (!_published.Add(fullPath))
                    return;

                _publishOrder.Enqueue(fullPath);

                // حذف قدیمی‌ترها به‌جای Clear کامل، تا dedup فایل‌های تازه از بین نرود
                while (_publishOrder.Count > MaxPublishedHistory)
                    _published.Remove(_publishOrder.Dequeue());
            }

            try
            {
                OnImageCaptured?.Invoke(lineId, fullPath, Stopwatch.GetTimestamp());
            }
            catch (Exception ex)
            {
                // استثنای مشترک نباید thread داخلی watcher را از کار بیندازد
                Log.Error(ex, "OnImageCaptured handler failed for {Path}", fullPath);
            }
        }

        public void Stop() => StopInternal(notify: true);

        private void StopInternal(bool notify)
        {
            List<LineWatcher> snapshot;

            lock (_linesLock)
            {
                snapshot = _lines.ToList();
                _lines.Clear();
            }

            foreach (var line in snapshot)
                DisposeWatcher(line);

            lock (_publishLock)
            {
                _published.Clear();
                _publishOrder.Clear();
            }

            if (notify)
                OnStatusChanged?.Invoke(false);
        }

        private static void DisposeWatcher(LineWatcher line)
        {
            try
            {
                line.Watcher.EnableRaisingEvents = false;
                line.Watcher.Dispose();
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Error disposing watcher for line {LineId}", line.LineId);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopInternal(notify: false);
        }
    }
}
