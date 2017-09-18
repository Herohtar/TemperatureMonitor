using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureMonitor
{
    public enum SensorStatus
    {
        Good,
        LowWarning,
        HighWarning,
        LowAlarm,
        HighAlarm,
        Error
    }
}
