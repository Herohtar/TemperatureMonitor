using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemperatureMonitor.Utilities;

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
            get
            {
                return time;
            }
            set
            {
                SetProperty(ref time, value);
            }
        }

        public double Temperature
        {
            get
            {
                return temperature;
            }
            set
            {
                SetProperty(ref temperature, value);
            }
        }
    }
}
