using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace TemperatureMonitor
{
    public class EntryPoint
    {
        [STAThread]
        public static void Main()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logPath = Path.Combine(appDataPath, "Herohtar", "TemperatureMonitor", "log-.txt");

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.Async(a => a.File(logPath, rollingInterval: RollingInterval.Day), blockWhenFull: true)
                .CreateLogger();

            Log.Information("Program start");

            Log.Information("Initializing database");
            using var db = new TemperatureSensorContext();
            db.Database.Migrate();

            try
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception e)
            {
                Log.Error(e, "Unhandled exception");
            }
            finally
            {
                Log.Information("Program end");
                Log.CloseAndFlush();
            }
        }
    }
}
