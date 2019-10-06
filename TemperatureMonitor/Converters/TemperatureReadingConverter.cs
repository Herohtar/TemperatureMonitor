using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TemperatureMonitor.Utilities;

namespace TemperatureMonitor
{
    public class TemperatureReadingConverter : BaseConverter, IValueConverter
    {
        public TemperatureReadingConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var readings = (List<TemperatureReading>)value;
            var figures = new PathFigureCollection();

            var cleanHistory = readings.Where(r => !double.IsNegativeInfinity(r.Temperature));
            if (cleanHistory.Count() > 0)
            {
                var startTime = cleanHistory.First().Time;
                var endTime = cleanHistory.Last().Time;
                var dataHeight = GraphSettings.Max - GraphSettings.Min;
                var dataWidth = (int)(endTime - startTime).TotalSeconds;
                var points = new PointCollection(cleanHistory.Select(r => new Point(((r.Time - startTime).TotalSeconds / dataWidth) * GraphSettings.Width, (1 - ((r.Temperature - GraphSettings.Min) / dataHeight)) * GraphSettings.Height)));

                var figure = new PathFigure()
                {
                    StartPoint = points.First()
                };
                figure.Segments.Add(new PolyLineSegment(points, true));
                figures.Add(figure);
            }

            return figures;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
