using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;
using VehicleWeightMeasurementSystemDemo.Reports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence
{
    public class SqlRepository
    {
        private readonly AppDbContext _context;

        public SqlRepository(AppDbContext context)
        {
            _context = context;
        }


        private IQueryable<VehicleEntity> BuildSearchQuery(
            DateTime from, DateTime to, int? lineId, string plate,
            double? minWeight, double? maxWeight, bool overweight)
        {
            var query = _context.Vehicles.AsNoTracking().AsQueryable();

            // همان بدنه فیلترهای فعلی SearchAsync (خطوط 182-225) را اینجا بگذار
            // date / lineId / plate / minWeight / maxWeight / overweight

            return query;
        }

        //public async Task<(List<VehicleReportDto> Items, int TotalCount)> SearchPagedAsync(
        //    DateTime from, DateTime to, int? lineId, string plate,
        //    double? minWeight, double? maxWeight, bool overweight,
        //    int page, int pageSize)
        //{
        //    if (page < 1) page = 1;
        //    if (pageSize < 1) pageSize = 50;

        //    var query = BuildSearchQuery(from, to, lineId, plate, minWeight, maxWeight, overweight);

        //    var total = await query.CountAsync();

        //    var items = await query
        //        .OrderByDescending(x => x.Timestamp)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(x => new VehicleReportDto
        //        {
        //            Id = x.Id,
        //            Timestamp = x.Timestamp,
        //            PlateNumber = x.PlateNumber,
        //            LineName = x.Line.LineName,
        //            Speed = x.Speed,
        //            TotalWeight = x.TotalWeight,
        //            AxleCount = x.AxleCount,
        //            Overweight = x.TotalOverWeight > 0
        //        })
        //        .ToListAsync();

        //    return (items, total);
        //}

        public async Task<(List<VehicleReportDto> Items, int TotalCount)> SearchPagedAsync(
            DateTime from, DateTime to, int? lineId, string plate,
            double? minWeight, double? maxWeight, bool overweight,
            int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            // بدون Skip/Take در دیتابیس
            var all = await SearchAsync(from, to, lineId, plate, minWeight, maxWeight, overweight);

            var items = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, all.Count);
        }


        public async Task<List<VehicleDto>> GetAllAsync()
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Include(v => v.Axles)   // 🔥 IMPORTANT
                .OrderByDescending(v => v.Timestamp)
                .Take(100)
                .Select(v => new VehicleDto
                    {
                        Id = v.Id,
                        Timestamp = v.Timestamp,
                        PlateNumber = v.PlateNumber,
                        Speed = v.Speed,
                        AxleCount = v.AxleCount,
                        TotalWeight = v.Axles.Sum(a => a.Weight),

                        LineId = v.LineId ?? 0,
                        LineName = v.Line.LineName,

                        ADC1 = v.ADC1,
                        ADC2 = v.ADC2,
                        ADC3 = v.ADC3,
                        ADC4 = v.ADC4,


                        // 🔥 MAP AXLES
                        AxleWeight1 = v.Axles.Where(a => a.AxleIndex == 1).Select(a => (double?)a.Weight).FirstOrDefault(),
                        AxleWeight2 = v.Axles.Where(a => a.AxleIndex == 2).Select(a => (double?)a.Weight).FirstOrDefault(),
                        AxleWeight3 = v.Axles.Where(a => a.AxleIndex == 3).Select(a => (double?)a.Weight).FirstOrDefault(),
                        AxleWeight4 = v.Axles.Where(a => a.AxleIndex == 4).Select(a => (double?)a.Weight).FirstOrDefault(),
                        AxleWeight5 = v.Axles.Where(a => a.AxleIndex == 5).Select(a => (double?)a.Weight).FirstOrDefault(),
                        AxleWeight6 = v.Axles.Where(a => a.AxleIndex == 6).Select(a => (double?)a.Weight).FirstOrDefault(),


                        Axle12 = v.Axles.Where(a => a.AxleIndex == 1).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),
                        Axle23 = v.Axles.Where(a => a.AxleIndex == 2).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),
                        Axle34 = v.Axles.Where(a => a.AxleIndex == 3).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),
                        Axle45 = v.Axles.Where(a => a.AxleIndex == 4).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),
                        Axle56 = v.Axles.Where(a => a.AxleIndex == 5).Select(a => (double?)Math.Round(a.Distance ?? 0, 2)).FirstOrDefault(),

                        // 🔥 ADD THIS
                        Axles = v.Axles
                                    .OrderBy(a => a.AxleIndex)
                                    .Select(a => new AxleDto
                                        {
                                            AxleIndex = a.AxleIndex,
                                            Weight = a.Weight,
                                            TimeMs = a.TimeMs ?? 0,
                                            Distance = a.Distance ?? 0
                                        }).ToList()
                                })
                                .ToListAsync();
        }

        public async Task<List<int>> GetActiveLineIdsAsync()
        {
            return await _context.Lines
                .Where(l => l.IsActive)
                .Select(l => l.Id)
                .ToListAsync();
        }

        public async Task<(int HasPlateCount, int TotalCount)> GetRecordCountsAsync()
        {
            var total = await _context.Vehicles.CountAsync();
            var hasPlate = await _context.Vehicles
                .CountAsync(v => v.PlateNumber != null &&
                                 v.PlateNumber != "" &&
                                 v.PlateNumber != "---");

            return (hasPlate, total);
        }

        public async Task SaveAsync(
            VehicleDto v,
            string imagePath,
            PlateResultDto plate)
        {
            using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                FileInfo file = null;

                if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                    file = new FileInfo(imagePath);

                string plateNo = plate?.PlateNumber ?? "";

                var vehicle = new VehicleEntity
                {
                    Timestamp = v.Timestamp == default ? DateTime.Now : v.Timestamp,

                    PlateNumber = plateNo,
                    Speed = v.Speed,
                    LineId = v.LineId,

                    AxleCount = v.AxleCount,
                    TotalWeight = v.TotalWeight,
                    AverageSpeed = v.Speed,

                    ADC1 = v.ADC1,
                    ADC2 = v.ADC2,
                    ADC3 = v.ADC3,
                    ADC4 = v.ADC4,

                    Axles = v.Axles.Select(a => new AxleEntity
                    {
                        AxleIndex = a.AxleIndex,
                        Weight = a.Weight,
                        TimeMs = a.TimeMs == 0 ? null : a.TimeMs,
                        Distance = a.Distance == 0 ? null : a.Distance,
                        LengthToNext = a.Distance == 0 ? null : a.Distance
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

                    // 🔥 Plate split SAFE
                    PlateP1 = plateNo.Length >= 2 ? plateNo.Substring(1, 2) : null,
                    PlateP2 = plateNo.Length >= 4 ? plateNo.Substring(4, 1) : null,
                    PlateP3 = plateNo.Length >= 6 ? plateNo.Substring(6, 3) : null,
                    PlateP4 = plateNo.Length > 6 ? plateNo.Substring(15, 2) : null,

                    PlateConfidence = plate?.Confidence,
                    PlateReadStatus = plate != null ? 1 : 0,
                    PlateReadAt = DateTime.Now,


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
            catch (Exception ex)
            {
                await trx.RollbackAsync();

                //var msg = ex.InnerException?.Message ?? ex.Message;
                //MessageBox.Show(msg, "DB ERROR");

                throw;
            }
        }

        public async Task<List<VehicleReportDto>> SearchAsync(
            DateTime from,
            DateTime to,
            int? lineId,
            string plate,
            double? minWeight,
            double? maxWeight,
            bool overweight)
        {

            var query = _context.Vehicles

                .Include(x => x.Line)
                .AsQueryable();



            query = query.Where(x =>
                x.Timestamp >= from &&
                x.Timestamp <= to.Date);


            if (lineId.HasValue)
            {
                query = query.Where(x => x.LineId == lineId);
            }


            if (!string.IsNullOrWhiteSpace(plate))
            {
                query = query.Where(x =>
                   x.PlateNumber.Contains(plate));
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


            return await query
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new VehicleReportDto
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                PlateNumber = x.PlateNumber,
                LineName = x.Line.LineName,
                Speed = x.Speed,
                TotalWeight = x.TotalWeight,
                AxleCount = x.AxleCount,
                Overweight = x.TotalOverWeight > 0
            })
            .ToListAsync();

        }

    }
}
