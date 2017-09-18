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
    public class SliderConverter : BaseConverter, IMultiValueConverter
    {
        public SliderConverter()
        {
        }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length >= 2)
            {
                double sliderValue = System.Convert.ToDouble(value[0]);
                double difference = (GraphSettings.End - GraphSettings.Start).TotalSeconds;

                return GraphSettings.Start.AddSeconds(difference * sliderValue).ToString();
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
