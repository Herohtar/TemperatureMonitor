using AlarmDotCom;
using AlarmDotCom.JsonObjects.ResponseData;
using MahApps.Metro.Controls.Dialogs;
using ReactiveComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TemperatureMonitor.Utilities;
using Serilog;

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

                    if (!success)
                    {
                        await progress.CloseAsync();
                        await dialogs.ShowMessageAsync(this, "Error", "There was an error logging you in, please try again.", MessageDialogStyle.Affirmative, new MetroDialogSettings { AnimateShow = false, AnimateHide = false });
                    }

                    if (progress.IsOpen)
                    {
                        await progress.CloseAsync();
                    }
                }
            }

            var sensorData = client.GetSensorData(0);
            sensorData.ForEach(sensor =>
            {
                Log.Information("Registering temperature sensor {SensorName}", sensor.Description);
                var newSensor = new TemperatureSensor(sensor.Description, maxHistory);
                newSensor.WhenTemperatureRecorded.Subscribe(r =>
                {
                    Max = (int)Sensors.SelectMany(s => s.TemperatureReadings).Max(reading => reading.Temperature) + GraphSettings.Buffer;
                    Min = (int)Sensors.SelectMany(s => s.TemperatureReadings).Where(reading => !double.IsNegativeInfinity(reading.Temperature)).Min(reading => reading.Temperature) - GraphSettings.Buffer;
                    StartTime = Sensors.SelectMany(s => s.TemperatureReadings).Min(reading => reading.Time);
                    EndTime = Sensors.SelectMany(s => s.TemperatureReadings).Max(reading => reading.Time);
                });

                Sensors.Add(newSensor);
            });

            recordTemperatures(sensorData);

            Observable.Interval(TimeSpan.FromMinutes(5), DispatcherScheduler.Current).Subscribe(x => recordTemperatures(client.GetSensorData(0)));

            Observable.Interval(TimeSpan.FromMinutes(1), DispatcherScheduler.Current).Subscribe(x => client.KeepAlive());

            IsStarted = true;
            Log.Information("Controller started");
        }

        private void recordTemperatures(List<TemperatureSensorsDatum> sensorData)
        {
            Log.Information("Recording temperature readings");
            var pollTime = DateTime.Now; // Reported reading times are inconsistent and incorrect in some cases, so just use the current time when recording
            sensorData.ForEach(sensor =>
            {
                Sensors.Single(s => s.Name.Equals(sensor.Description)).RecordTemperature(sensor.LastKnownReading, pollTime);
            });
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
