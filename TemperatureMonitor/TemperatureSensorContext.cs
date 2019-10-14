using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TemperatureMonitor
{
    public class TemperatureSensorContext : DbContext
    {
        public DbSet<TemperatureSensor> TemperatureSensors { get; set; } = null!;

        private readonly string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Herohtar", "TemperatureMonitor", "readings.db");

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={dbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TemperatureSensor>()
                .Ignore(s => s.IsDisposed)
                .Property(s => s.Type)
                .HasConversion<string>();

            modelBuilder.Entity<TemperatureReading>()
                .Ignore(r => r.IsDisposed);
        }
    }
}
