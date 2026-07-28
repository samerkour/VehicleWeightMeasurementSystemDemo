using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Domain.Entities;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence
{
    public class SqlRepository
    {
        private readonly AppDbContext _context;

        public SqlRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<VehicleDto>> GetAllAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Axles)   // 🔥 IMPORTANT
                .OrderByDescending(v => v.Timestamp)
                .Take(100)
                .Select(v => new VehicleDto
                    {
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
    }
}
