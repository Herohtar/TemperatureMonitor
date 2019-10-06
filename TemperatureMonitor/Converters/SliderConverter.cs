using System;
using System.Globalization;
using System.Windows;
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
                var sliderValue = System.Convert.ToDouble(value[0]);
                var difference = (GraphSettings.End - GraphSettings.Start).TotalSeconds;

                return GraphSettings.Start.AddSeconds(difference * sliderValue).ToString();
            }

            return DependencyProperty.UnsetValue;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
