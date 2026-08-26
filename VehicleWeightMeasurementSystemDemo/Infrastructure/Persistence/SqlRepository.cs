using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Controls;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;
using VehicleWeightMeasurementSystemDemo.Reports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence
{
    public class SqlRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public SqlRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        private async Task<AppDbContext> CreateContextAsync()
        {
            return await _contextFactory.CreateDbContextAsync();
        }

        // 🔥 پروجکشن مشترک گزارش (یک‌بار تعریف، در همه‌ی کوئری‌ها استفاده می‌شود)
        private static readonly Expression<Func<VehicleEntity, VehicleReportDto>> VehicleReportSelector =
            x => new VehicleReportDto
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                PlateNumber = x.PlateNumber,
                LineName = x.Line.LineName,
                Speed = x.Speed,
                TotalWeight = x.TotalWeight,
                AxleCount = x.AxleCount,
                Overweight = x.TotalOverWeight > 0,

                ADC1 = x.ADC1,
                ADC2 = x.ADC2,
                ADC3 = x.ADC3,
                ADC4 = x.ADC4,

                AxleWeight1 = x.Axles.Where(a => a.AxleIndex == 1).Select(a => (double?)a.Weight).FirstOrDefault(),
                AxleWeight2 = x.Axles.Where(a => a.AxleIndex == 2).Select(a => (double?)a.Weight).FirstOrDefault(),
                AxleWeight3 = x.Axles.Where(a => a.AxleIndex == 3).Select(a => (double?)a.Weight).FirstOrDefault(),
                AxleWeight4 = x.Axles.Where(a => a.AxleIndex == 4).Select(a => (double?)a.Weight).FirstOrDefault(),
                AxleWeight5 = x.Axles.Where(a => a.AxleIndex == 5).Select(a => (double?)a.Weight).FirstOrDefault(),
                AxleWeight6 = x.Axles.Where(a => a.AxleIndex == 6).Select(a => (double?)a.Weight).FirstOrDefault(),

                Axle12 = x.Axles.Where(a => a.AxleIndex == 1).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),
                Axle23 = x.Axles.Where(a => a.AxleIndex == 2).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),
                Axle34 = x.Axles.Where(a => a.AxleIndex == 3).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),
                Axle45 = x.Axles.Where(a => a.AxleIndex == 4).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),
                Axle56 = x.Axles.Where(a => a.AxleIndex == 5).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault()
            };

        /// <summary>
        /// ساخت کوئری جستجو با فیلترها (بدون ماده‌سازی).
        /// Context همراه کوئری برگردانده می‌شود تا تا پایان اجرای کوئری زنده بماند و Dispose شود.
        /// </summary>
        private async Task<(AppDbContext Context, IQueryable<VehicleEntity> Query)> BuildSearchQuery(
            DateTime from, DateTime to, int? lineId, string plate,
            double? minWeight, double? maxWeight, bool overweight)
        {
            var _context = await CreateContextAsync();
            var query = _context.Vehicles.AsNoTracking().AsQueryable();

            // 🔥 فیلتر بازه‌ی تاریخ
            query = query.Where(x =>
                x.Timestamp >= from &&
                x.Timestamp <= to);

            if (lineId.HasValue && lineId != 0)
            {
                query = query.Where(x => x.LineId == lineId);
            }

            if (!string.IsNullOrWhiteSpace(plate))
            {
                string term = PersianCalendarHelper.NormalizePlate(plate);

                query = query.Where(x =>
                    x.PlateNumber
                        .Replace("\u200F", "")
                        .Replace("\u200E", "")
                        .Replace("\u200B", "")
                        .Replace("\u200C", "")
                        .Replace("\u200D", "")
                        .Replace("۰", "0").Replace("۱", "1").Replace("۲", "2").Replace("۳", "3")
                        .Replace("۴", "4").Replace("۵", "5").Replace("۶", "6").Replace("۷", "7")
                        .Replace("۸", "8").Replace("۹", "9")
                        .Replace("٠", "0").Replace("١", "1").Replace("٢", "2").Replace("٣", "3")
                        .Replace("٤", "4").Replace("٥", "5").Replace("٦", "6").Replace("٧", "7")
                        .Replace("٨", "8").Replace("٩", "9")
                        .Replace("  ", " ")
                        .Trim()
                        .Contains(term));
            }

            if (minWeight.HasValue && minWeight != 0)
            {
                query = query.Where(x =>
                   x.TotalWeight >= minWeight);
            }

            if (maxWeight.HasValue && maxWeight != 0)
            {
                query = query.Where(x =>
                   x.TotalWeight <= maxWeight);
            }

            if (overweight)
            {
                query = query.Where(x =>
                   x.TotalOverWeight > 0);
            }

            return (_context, query);
        }

        public async Task<(List<VehicleReportDto> Items, int TotalCount)> SearchPagedAsync(
            DateTime from, DateTime to, int? lineId, string plate,
            double? minWeight, double? maxWeight, bool overweight,
            int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var (context, query) = await BuildSearchQuery(
                from, to, lineId, plate, minWeight, maxWeight, overweight);

            await using var _ = context;

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(VehicleReportSelector)
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<int>> GetActiveLineIdsAsync()
        {
            await using var _context = await CreateContextAsync();

            return await _context.Lines
                .Where(l => l.IsActive)
                .Select(l => l.Id)
                .ToListAsync();
        }

        public async Task<(int HasPlateCount, int TotalCount)> GetRecordCountsAsync(DateTime? date = null)
        {
            await using var _context = await CreateContextAsync();

            var dayStart = (date ?? DateTime.Now).Date;
            var dayEnd = dayStart.AddDays(1);

            var total = await _context.Vehicles
                .CountAsync(v => v.Timestamp >= dayStart && v.Timestamp < dayEnd);
            var hasPlate = await _context.Vehicles
                .CountAsync(v => v.Timestamp >= dayStart && v.Timestamp < dayEnd &&
                                 v.PlateNumber != null &&
                                 v.PlateNumber != "");

            return (hasPlate, total);
        }

        public async Task SaveAsync(
            VehicleDto v,
            string imagePath,
            PlateResultDto plate,
            decimal axleAlpha = 1.0m,
            decimal weightAlpha = 1.5m)
        {
            await using var _context = await CreateContextAsync();

            using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                FileInfo file = null;

                if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                    file = new FileInfo(imagePath);

                string plateNo = plate?.PlateNumber ?? "";

                // 🔥 Split plate into parts: format is {part3} ایران {part2} {letter} {part1}
                // e.g. "۱۱ ایران ۳۴۵ ب ۱۲" → part1="۱۲", part2="ب", part3="۳۴۵", part4="۱۱"
                string[] plateParts = new string?[4] { null, null, null, null };
                string[] tokens = plateNo
                    .Replace("\u200F", "")
                    .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(t => t != "ایران")
                    .ToArray();
                if (tokens.Length == 4)
                {
                    // tokens[0]=part3(۱۱), tokens[1]=part2(۳۴۵), tokens[2]=letter(ب), tokens[3]=part1(۱۲)
                    plateParts = tokens;
                }

                var vehicle = new VehicleEntity
                {
                    Timestamp = v.Timestamp == default ? DateTime.Now : v.Timestamp,

                    PlateNumber = plateNo,
                    Speed = v.Speed,
                    LineId = v.LineId,

                    AxleCount = v.AxleCount,
                    TotalWeight = v.TotalWeight == null ? null : (double?)(int)Math.Round((decimal)v.TotalWeight.Value * weightAlpha),
                    AverageSpeed = v.Speed,

                    ADC1 = v.ADC1,
                    ADC2 = v.ADC2,
                    ADC3 = v.ADC3,
                    ADC4 = v.ADC4,

                    // 🔥 فیلدهای جامع (CarClass13 و هم‌خانواده)
                    PlateConfidence = v.PlateConfidence ?? plate?.Confidence,
                    PlateReadStatus = v.PlateReadStatus ?? (plate != null && !string.IsNullOrEmpty(plate.PlateNumber) ? 1 : 0),
                    PlateReadAt = v.PlateReadAt,
                    Allowed = v.Allowed,
                    WrongDirection = v.WrongDirection,
                    VehicleClass = v.VehicleClass,
                    Longitude = v.Longitude,
                    Latitude = v.Latitude,
                    VehicleLen = v.VehicleLen,
                    TotalOverWeight = v.TotalOverWeight,

                    SpeedType = v.SpeedType,
                    CrimeCodes = v.CrimeCodes,

                    Axles = v.Axles.Select(a => new AxleEntity
                    {
                        AxleIndex = a.AxleIndex,
                        Weight = a.Weight.HasValue ? (int)Math.Round((decimal)a.Weight.Value * weightAlpha) : 0,
                        TimeMs = a.TimeMs == 0 ? null : a.TimeMs,
                        Distance = a.Distance == 0 ? null : (double?)(a.Distance * (double)axleAlpha),
                        LengthToNext = a.Distance == 0 ? null : (double?)(a.Distance * (double)axleAlpha)
                    }).ToList()
                };

                // 🔥 Add CameraPhoto correctly (1-to-many)
                var photo = new CameraPhotosEntity
                {
                    FileName = file?.Name ?? "-",
                    FullPath = file?.FullName ?? "-",
                    RelativePath = file?.Name ?? "-",
                    FileSizeBytes = file?.Length ?? 0,

                    CapturedAt = file?.CreationTime ?? DateTime.Now,
                    ImportedAt = DateTime.Now,

                    // 🔥 Plate split SAFE (columns are NOT NULL → never pass null)
                    PlateP1 = plateParts[3] ?? "",
                    PlateP2 = plateParts[2] ?? "",
                    PlateP3 = plateParts[1] ?? "",
                    PlateP4 = plateParts[0] ?? "",

                    // 🔥 تصویر کراپ‌شده پلاک (C:\Temp\RahdariImages)
                    PlateFileName = plate?.PlateFileName,
                    PlateRelativePath = plate?.PlateRelativePath,
                    PlateFullPath = plate?.PlateFullPath,


                    TerminalSent = false,
                    TerminalTtoRegistered = false,
                    TerminalImageExpired = false,
                };

                // 🔥 Link properly
                vehicle.Photos.Add(photo);

                _context.Vehicles.Add(vehicle);

                await _context.SaveChangesAsync();
                await trx.CommitAsync();
            }
            catch (Exception)
            {
                await trx.RollbackAsync();

                //var msg = ex.InnerException?.Message ?? ex.Message;
                //MessageBox.Show(msg, "DB ERROR");

                throw;
            }
        }

        public async Task<string?> GetVehiclePhotoPathAsync(int vehicleId)
        {
            await using var _context = await CreateContextAsync();

            return await _context.CameraPhotos
                .Where(p => p.VehicleId == vehicleId)
                .OrderByDescending(p => p.Id)
                .Select(p => p.FullPath)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetVehiclePlateImagePathAsync(int vehicleId)
        {
            await using var _context = await CreateContextAsync();

            return await _context.CameraPhotos
                .Where(p => p.VehicleId == vehicleId && p.PlateFullPath != null)
                .OrderByDescending(p => p.Id)
                .Select(p => p.PlateFullPath)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// مسیر جدیدترین کراپ پلاک هر خودرو در «یک» کوئری (به‌جای N+1).
        /// </summary>
        public async Task<Dictionary<int, string>> GetVehiclePlateImagePathsAsync(
            IEnumerable<int> vehicleIds)
        {
            var ids = vehicleIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, string>();

            await using var _context = await CreateContextAsync();

            var rows = await _context.CameraPhotos
                .Where(p => p.VehicleId != null &&
                            p.PlateFullPath != null &&
                            ids.Contains(p.VehicleId.Value))
                .OrderByDescending(p => p.Id)
                .Select(p => new { VehicleId = p.VehicleId!.Value, Path = p.PlateFullPath! })
                .ToListAsync();

            // اولین ردیف هر خودرو = جدیدترین عکس
            var result = new Dictionary<int, string>(rows.Count);
            foreach (var row in rows)
                if (!result.ContainsKey(row.VehicleId))
                    result[row.VehicleId] = row.Path;

            return result;
        }

        /// <summary>
        /// مسیر عکس اصلی هر خودرو در «یک» کوئری (به‌جای N+1).
        /// </summary>
        public async Task<Dictionary<int, string>> GetVehiclePhotoPathsAsync(
            IEnumerable<int> vehicleIds)
        {
            var ids = vehicleIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, string>();

            await using var _context = await CreateContextAsync();

            var rows = await _context.CameraPhotos
                .Where(p => p.VehicleId != null &&
                            p.FullPath != null &&
                            ids.Contains(p.VehicleId.Value))
                .OrderByDescending(p => p.Id)
                .Select(p => new { VehicleId = p.VehicleId!.Value, Path = p.FullPath! })
                .ToListAsync();

            var result = new Dictionary<int, string>(rows.Count);
            foreach (var row in rows)
                if (!result.ContainsKey(row.VehicleId))
                    result[row.VehicleId] = row.Path;

            return result;
        }

        public async Task<(List<VehicleReportDto> Items, int TotalCount)> SearchAsync(
            DateTime from,
            DateTime to,
            int? lineId,
            string plate,
            double? minWeight,
            double? maxWeight,
            bool overweight,
            int page = 1,
            int pageSize = 100,
            bool hasPlate = false)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;

            var (context, query) = await BuildSearchQuery(
                from, to, lineId, plate, minWeight, maxWeight, overweight);

            await using var _ = context;



            if (hasPlate)
            {
                // همان شرط شمارش HasPlate در GetRecordCountsAsync
                query = query.Where(x => !string.IsNullOrEmpty(x.PlateNumber) || !string.IsNullOrWhiteSpace (x.PlateNumber));
            }


            var totalCount = await query.CountAsync();

            // 🔥 SQL Server 2019 → صفحه‌بندی سمت دیتابیس (OFFSET/FETCH)
            var items = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(VehicleReportSelector)
                .ToListAsync();

            return (items, totalCount);
        }

    }
}
