using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TemperatureMonitor.Utilities;
using System.Reactive.Concurrency;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls.Dialogs;

namespace TemperatureMonitor
{
    public class Controller : NotifyDataErrorInfo<Controller>
    {
        private IDialogCoordinator dialogs;
        private ObservableCollection<TemperatureSensor> sensors;
        private AlarmDotComWebClient client;

        private const int maxHistory = 0;

        private bool isStarted = false;

        public Controller(IDialogCoordinator dialogCoordinator)
        {
            dialogs = dialogCoordinator;
            sensors = new ObservableCollection<TemperatureSensor>();

            GraphSettings.Buffer = 2;
        }

        public async void Start()
        {
            LoginDialogData data;

            bool success = false;
            while (!success)
            {
                data = await dialogs.ShowLoginAsync(this, "Login", String.Empty, new LoginDialogSettings { AnimateShow = false, AnimateHide = false, NegativeButtonText = "Exit", NegativeButtonVisibility = Visibility.Visible });
                if (data == null)
                {
                    Application.Current.Shutdown();
                    return;
                }
                else
                {
                    ProgressDialogController progress = await dialogs.ShowProgressAsync(this, "Logging in...", "Connecting to Alarm.com", false, new MetroDialogSettings { AnimateShow = false, AnimateHide = false });
                    progress.SetIndeterminate();

                    client = new AlarmDotComWebClient(data.Username, data.Password);
                    try
                    {
                        await Task.Run(() => client.Login());
                        success = true;
                    }
                    catch
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

            List<TemperatureSensorsData> sensorData = client.GetSensorData(0);
            sensorData.ForEach(sensor =>
            {
                TemperatureSensor newSensor = new TemperatureSensor(sensor.Description, maxHistory);
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

            IsStarted = true;
        }

        private void recordTemperatures(List<TemperatureSensorsData> sensorData)
        {
            DateTime pollTime = DateTime.Now; // Reported reading times are inconsistent and incorrect in some cases, so just use the current time when recording
            sensorData.ForEach(sensor =>
            {
                Sensors.Single(s => s.Name.Equals(sensor.Description)).RecordTemperature(sensor.LastKnownReading, pollTime);
            });
        }

        public ObservableCollection<TemperatureSensor> Sensors
        {
            get { return sensors; }
        }

        public bool IsStarted
        {
            get { return isStarted; }
            set { SetProperty(ref isStarted, value); }
        }

        public DateTime StartTime
        {
            get { return GraphSettings.Start; }
            set { SetProperty(() => GraphSettings.Start == value, () => GraphSettings.Start = value); }
        }

        public DateTime EndTime
        {
            get { return GraphSettings.End; }
            set { SetProperty(() => GraphSettings.End == value, () => GraphSettings.End = value); }
        }

        public double GraphHeight
        {
            get { return GraphSettings.Height; }
            set { SetProperty(() => GraphSettings.Height == value, () => GraphSettings.Height = value); }
        }

        public double GraphWidth
        {
            get { return GraphSettings.Width; }
            set { SetProperty(() => GraphSettings.Width == value, () => GraphSettings.Width = value); }
        }

        public int Max
        {
            get { return GraphSettings.Max; }
            set { SetProperty(() => GraphSettings.Max == value, () => GraphSettings.Max = value); }
        }

        public int Min
        {
            get { return GraphSettings.Min; }
            set { SetProperty(() => GraphSettings.Min == value, () => GraphSettings.Min = value); }
        }

        public event EventHandler TrayClick;
        public ICommand TrayClickCommand
        {
            get
            {
                return new Command
                {
                    ExecuteDelegate = p =>
                    {
                        TrayClick?.Invoke(this, new EventArgs());
                    },
                    CanExecuteDelegate = p =>
                    {
                        return true;
                    }
                };
            }
        }
    }
}
