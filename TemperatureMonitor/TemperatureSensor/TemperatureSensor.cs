using ReactiveComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TemperatureMonitor
{
    public class TemperatureSensor : NotifyDataErrorInfo<TemperatureSensor>
    {
        private string name;
        private int maxHistory;
        private Subject<TemperatureReading> temperatureRecorded = new Subject<TemperatureReading>();

        public TemperatureSensor(string sensorName, int maxHistoryCount)
        {
            name = sensorName;

            maxHistory = maxHistoryCount;

            TemperatureReadings = new List<TemperatureReading>();
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public IObservable<TemperatureReading> WhenTemperatureRecorded => temperatureRecorded;

        public void RecordTemperature(double temperature) => RecordTemperature(new TemperatureReading(DateTime.Now, temperature));

        public void RecordTemperature(double temperature, DateTime time) => RecordTemperature(new TemperatureReading(time, temperature));

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

        public List<TemperatureReading> TemperatureReadings { get; }

        public double CurrentTemperature => TemperatureReadings.DefaultIfEmpty(new TemperatureReading(DateTime.Now, double.NaN)).Last().Temperature;
    }
}
