using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TemperatureMonitor.Utilities;

namespace TemperatureMonitor
{
    public class TemperatureFormatConverter : BaseConverter, IValueConverter
    {
        public TemperatureFormatConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double temperature = (double)value;
            if (double.IsNaN(temperature))
            {
                return "---";
            }

            if (double.IsNegativeInfinity(temperature))
            {
                return "Err";
            }

            return temperature.ToString("0").PadLeft(3, '!');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
