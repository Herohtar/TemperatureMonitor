using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TemperatureMonitor.Utilities;

namespace TemperatureMonitor
{
    public class TemperatureSensor : NotifyDataErrorInfo<TemperatureSensor>
    {
        private string name;

        private int maxHistory;

        private const int buffer = 2;

        private Subject<TemperatureReading> temperatureRecorded = new Subject<TemperatureReading>();

        private List<TemperatureReading> temperatureReadings;

        public TemperatureSensor(string sensorName, int maxHistoryCount)
        {
            name = sensorName;

            maxHistory = maxHistoryCount;

            temperatureReadings = new List<TemperatureReading>();
        }

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public IObservable<TemperatureReading> WhenTemperatureRecorded
        {
            get { return temperatureRecorded; }
        }

        public void RecordTemperature(double temperature)
        {
            RecordTemperature(new TemperatureReading(DateTime.Now, temperature));
        }

        public void RecordTemperature(double temperature, DateTime time)
        {
            RecordTemperature(new TemperatureReading(time, temperature));
        }

        public void RecordTemperature(TemperatureReading reading)
        {
            if (!TemperatureReadings.Any(h => h.Time.Equals(reading.Time)))
            {
                TemperatureReadings.Add(reading);
                if ((maxHistory > 0) && (TemperatureReadings.Count > maxHistory))
                {
                    TemperatureReadings.RemoveAt(0);
                }

                temperatureRecorded.OnNext(reading);
                OnPropertyChanged("CurrentTemperature", "TemperatureReadings");
            }
        }

        public List<TemperatureReading> TemperatureReadings
        {
            get { return temperatureReadings; }
        }

        public double CurrentTemperature
        {
            get { return TemperatureReadings.DefaultIfEmpty(new TemperatureReading(DateTime.Now, double.NaN)).Last().Temperature; }
        }
    }
}
