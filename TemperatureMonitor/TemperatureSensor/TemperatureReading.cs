using ReactiveComponentModel;
using System;

namespace TemperatureMonitor
{
    public class TemperatureReading : NotifyPropertyChanges
    {
        private DateTime time;
        private double temperature;

        public TemperatureReading(DateTime time, double temperature)
        {
            this.time = time;
            this.temperature = temperature;
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
    }
}
