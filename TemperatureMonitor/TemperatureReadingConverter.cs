using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            List<TemperatureReading> readings = (List<TemperatureReading>)value;
            PathFigureCollection figures = null;

            IEnumerable<TemperatureReading> cleanHistory = readings.Where(r => !double.IsNegativeInfinity(r.Temperature));
            if (cleanHistory.Count() > 0)
            {
                DateTime startTime = cleanHistory.First().Time;
                DateTime endTime = cleanHistory.Last().Time;
                int dataHeight = GraphSettings.Max - GraphSettings.Min;
                int dataWidth = (int)(endTime - startTime).TotalSeconds;
                PointCollection points = new PointCollection(cleanHistory.Select(r => new Point(((r.Time - startTime).TotalSeconds / dataWidth) * GraphSettings.Width, (1 - ((r.Temperature - GraphSettings.Min) / dataHeight)) * GraphSettings.Height)));

                figures = new PathFigureCollection();
                PathFigure figure = new PathFigure()
                {
                    StartPoint = points.First()
                };
                figure.Segments.Add(new PolyLineSegment(points, true));
                figures.Add(figure);
            }

            return figures;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
