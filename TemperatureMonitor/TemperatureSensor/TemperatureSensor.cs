using ReactiveComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Serilog;

namespace TemperatureMonitor
{
    public class TemperatureSensor : NotifyDataErrorInfo<TemperatureSensor>
    {
        private string id;
        private SensorType type;
        private string name;
        private int maxHistory;
        private Subject<TemperatureReading> temperatureRecorded = new Subject<TemperatureReading>();

        public TemperatureSensor(string id, SensorType type, string name) : this(id, type, name, 0)
        {

        }
        
        public TemperatureSensor(string id, SensorType type, string name, int maxHistory)
        {
            Log.Debug("TemperatureSensor instance created for {SensorName} with max history of {MaxHistory}", name, maxHistory);

            this.id = id;

            this.type = type;
            
            this.name = name;

            this.maxHistory = maxHistory;

            TemperatureReadings = new List<TemperatureReading>();
        }

        public string Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public SensorType Type
        {
            get => type;
            set => SetProperty(ref type, value);
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
                Log.Information("Recording temperature {Temperature} for {Name}", reading.Temperature, Name);
                TemperatureReadings.Add(reading);
                if ((maxHistory > 0) && (TemperatureReadings.Count > maxHistory))
                {
                    Log.Information("Max history reached; removing oldest reading from {Time}", TemperatureReadings.ElementAt(0).Time);
                    TemperatureReadings.RemoveAt(0);
                }

                temperatureRecorded.OnNext(reading);
                OnPropertyChanged("CurrentTemperature", "TemperatureReadings");
            }
            else
            {
                Log.Information("{Name} already has a temperature entry for {Time}, not recording", Name, reading.Time);
            }
        }

        public List<TemperatureReading> TemperatureReadings { get; }

        public double CurrentTemperature => TemperatureReadings.DefaultIfEmpty(new TemperatureReading(DateTime.Now, double.NaN)).Last().Temperature;
    }
}
