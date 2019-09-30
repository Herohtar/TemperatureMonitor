using AlarmDotCom;
using MahApps.Metro.Controls.Dialogs;
using ReactiveComponentModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TemperatureMonitor
{
    public class Controller : NotifyDataErrorInfo<Controller>
    {
        private readonly IDialogCoordinator dialogs;
        private Client client;

        private const int maxHistory = 0;

        private bool isStarted = false;

        public Controller(IDialogCoordinator dialogCoordinator)
        {
            Log.ForContext<Controller>();
            Log.Debug("Instance of Controller created");

            dialogs = dialogCoordinator;
            Sensors = new ObservableCollection<TemperatureSensor>();

            GraphSettings.Buffer = 2;
        }

        public async void Start()
        {
            Log.Information("Starting controller");

            var success = false;
            while (!success)
            {
                Log.Information("Showing login dialog");
                var data = await dialogs.ShowLoginAsync(this, "Login", string.Empty, new LoginDialogSettings { AnimateShow = false, AnimateHide = false, NegativeButtonText = "Exit", NegativeButtonVisibility = Visibility.Visible });
                if (data == null)
                {
                    Log.Information("Login canceled - exiting");
                    Application.Current.Shutdown();
                    return;
                }
                else
                {
                    Log.Information("Got login details");
                    var progress = await dialogs.ShowProgressAsync(this, "Logging in...", "Connecting to Alarm.com", false, new MetroDialogSettings { AnimateShow = false, AnimateHide = false });
                    progress.SetIndeterminate();

                    client = new Client(data.Username, data.Password);

                    success = await Task.Run(() => client.Login());

                    await progress.CloseAsync();

                    if (!success)
                    {
                        await dialogs.ShowMessageAsync(this, "Error", "There was an error logging you in, please try again.", MessageDialogStyle.Affirmative, new MetroDialogSettings { AnimateShow = false, AnimateHide = false });
                    }
                }
            }

            var sensors = getTemperatureSensors();
            sensors.ForEach(sensor =>
            {
                Log.Information("Registering temperature sensor {SensorName}", sensor.Name);
                sensor.WhenTemperatureRecorded.Subscribe(r =>
                {
                    var readings = Sensors.SelectMany(s => s.TemperatureReadings);
                    Max = (int)readings.Max(reading => reading.Temperature) + GraphSettings.Buffer;
                    Min = (int)readings.Where(reading => !double.IsNegativeInfinity(reading.Temperature)).Min(reading => reading.Temperature) - GraphSettings.Buffer;
                    StartTime = readings.Min(reading => reading.Time);
                    EndTime = readings.Max(reading => reading.Time);
                });

                Sensors.Add(sensor);
            });

            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(5), DispatcherScheduler.Current).Subscribe(x => updateTemperatures());

            Observable.Interval(TimeSpan.FromMinutes(1), DispatcherScheduler.Current).Subscribe(x => client.KeepAlive());

            IsStarted = true;
            Log.Information("Controller started");
        }

        private List<TemperatureSensor> getTemperatureSensors()
        {
            Log.Information("Getting all thermostats and temperature sensors");
            var systems = from system in client.GetAvailableSystems()
                          select client.GetSystemData(system);

            var thermostats = from system in systems
                              from thermostatItem in system.Relationships.Thermostats.Data
                              select client.GetThermostatData(thermostatItem.Id);

            var temperatureSensors = from system in systems
                                     from temperatureSensorItem in system.Relationships.RemoteTemperatureSensors.Data
                                     select client.GetTemperatureSensorData(temperatureSensorItem.Id);

            return (
                    from thermostat in thermostats
                    select new TemperatureSensor(thermostat.Id, thermostat.Type, thermostat.Attributes.Description, maxHistory)
                   ).Concat(
                    from temperatureSensor in temperatureSensors
                    select new TemperatureSensor(temperatureSensor.Id, temperatureSensor.Type, temperatureSensor.Attributes.Description, maxHistory)
                   ).ToList();
        }

        private void updateTemperatures()
        {
            Log.Information("Updating temperature readings");
            var pollTime = DateTime.Now; // Use a constant time across all readings

            foreach (var sensor in Sensors)
            {
                switch (sensor.Type)
                {
                    case SensorType.Thermostat:
                        sensor.RecordTemperature(client.GetThermostatData(sensor.Id).Attributes.AmbientTemp, pollTime);
                        break;
                    case SensorType.RemoteTemperatureSensor:
                        sensor.RecordTemperature(client.GetTemperatureSensorData(sensor.Id).Attributes.AmbientTemp, pollTime);
                        break;
                }
            }
        }

        public ObservableCollection<TemperatureSensor> Sensors { get; }

        public bool IsStarted
        {
            get => isStarted;
            set => SetProperty(ref isStarted, value);
        }

        public DateTime StartTime
        {
            get => GraphSettings.Start;
            set => SetProperty(() => GraphSettings.Start == value, () => GraphSettings.Start = value);
        }

        public DateTime EndTime
        {
            get => GraphSettings.End;
            set => SetProperty(() => GraphSettings.End == value, () => GraphSettings.End = value);
        }

        public double GraphHeight
        {
            get => GraphSettings.Height;
            set => SetProperty(() => GraphSettings.Height == value, () => GraphSettings.Height = value);
        }

        public double GraphWidth
        {
            get => GraphSettings.Width;
            set => SetProperty(() => GraphSettings.Width == value, () => GraphSettings.Width = value);
        }

        public int Max
        {
            get => GraphSettings.Max;
            set => SetProperty(() => GraphSettings.Max == value, () => GraphSettings.Max = value);
        }

        public int Min
        {
            get => GraphSettings.Min;
            set => SetProperty(() => GraphSettings.Min == value, () => GraphSettings.Min = value);
        }
    }
}
