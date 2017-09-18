using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemperatureMonitor.Utilities;
using System.Reactive.Linq;

namespace TemperatureMonitor
{
    public class TemperatureSettings
    {
        public TemperatureSettings(double highAlarm, double highOffset, double lowAlarm, double lowOffset)
        {
            HighAlarmPoint = highAlarm;
            HighWarningOffset = highOffset;
            LowAlarmPoint = lowAlarm;
            LowWarningOffset = lowOffset;
            IsModified = false;
        }

        public bool IsModified
        {
            get;
            set;
        }

        public double HighAlarmPoint
        {
            get;
            set;
        }

        public double HighWarningOffset
        {
            get;
            set;
        }

        public double LowAlarmPoint
        {
            get;
            set;
        }

        public double LowWarningOffset
        {
            get;
            set;
        }
    }
}
