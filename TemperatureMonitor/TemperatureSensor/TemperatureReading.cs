using ReactiveComponentModel;
using System;

namespace TemperatureMonitor
{
    public class TemperatureReading : NotifyPropertyChanges
    {
        private int id;
        private DateTime time;
        private double temperature;
        private string temperatureSensorId = null!;

        public TemperatureReading(DateTime time, double temperature)
        {
            this.time = time;
            this.temperature = temperature;
        }

        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public DateTime Time
        {
            get => time;
            set => SetProperty(ref time, value);
        }

        public double Temperature
        {
            get => temperature;
            set => SetProperty(ref temperature, value);
        }

        public string TemperatureSensorId
        {
            get => temperatureSensorId;
            set => SetProperty(ref temperatureSensorId, value);
        }
    }
}
