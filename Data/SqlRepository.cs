using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleWeightMeasurementSystemDemo.Models;

namespace VehicleWeightMeasurementSystemDemo.Data
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
                        TotalWeight = v.TotalWeight,
                        Line = v.Line,
                        ADC1 = v.ADC1,
                        ADC2 = v.ADC2,
                        ADC3 = v.ADC3,
                        ADC4 = v.ADC4,

                        // 🔥 ADD THIS
                        Axles = v.Axles.Select(a => new AxleDto
                        {
                            Index = a.AxleIndex,
                            Weight = a.Weight,
                            TimeMs = a.TimeMs,
                            Distance = a.Distance
                        }).ToList()
                    })
                    .ToListAsync();
        }

        public async Task SaveAsync(VehicleDto v)
        {
            var entity = new VehicleEntity
            {
                
                PlateNumber = v.PlateNumber,
                Speed = v.Speed,
                Line = v.Line,
                AxleCount = v.AxleCount,
                TotalWeight = v.TotalWeight,
                ADC1 = v.ADC1,
                ADC2 = v.ADC2,
                ADC3 = v.ADC3,
                ADC4 = v.ADC4,
                Timestamp = v.Timestamp,
                Axles = v.Axles.Select(a => new AxleEntity
                {
                    AxleIndex = a.Index,
                    Weight = a.Weight,
                    TimeMs = a.TimeMs == 0 ? null : a.TimeMs,
                    Distance = a.Distance == 0 ? null : a.Distance
                }).ToList()
            };

            _context.Vehicles.Add(entity);
            await _context.SaveChangesAsync();
        }
    }
}
