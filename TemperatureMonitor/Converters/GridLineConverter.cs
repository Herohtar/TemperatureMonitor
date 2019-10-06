using System;
using System.Collections.Generic;
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
    public class GridLineConverter : BaseConverter, IMultiValueConverter
    {
        public GridLineConverter()
        {
        }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var figures = new PathFigureCollection();

            if (value.Length >= 2)
            {
                double lowerValue = System.Convert.ToDouble(value[0]);
                double upperValue = System.Convert.ToDouble(value[1]);
                double scale = upperValue - lowerValue;
                double interval;

                PathFigure figure;
                double dataWidth = (GraphSettings.End - GraphSettings.Start).TotalSeconds;
                if (dataWidth > 0)
                {
                    figure = new PathFigure();
                    interval = findInterval(dataWidth * scale, 15);
                    for (double x = interval - (GraphSettings.Start.Second % interval); x < dataWidth; x += interval)
                    {
                        double scaledX = (x / dataWidth) * GraphSettings.Width;
                        figure.Segments.Add(new LineSegment(new Point(scaledX, 0), false));
                        figure.Segments.Add(new LineSegment(new Point(scaledX, GraphSettings.Height), true));
                    }
                    figures.Add(figure);
                }

                double dataHeight = GraphSettings.Max - GraphSettings.Min;
                if (dataHeight > 0)
                {
                    figure = new PathFigure();
                    interval = findInterval(dataHeight, 5);
                    for (double y = interval - (GraphSettings.Min % interval); y < dataHeight; y += interval)
                    {
                        double scaledY = (1 - (y / dataHeight)) * GraphSettings.Height;
                        figure.Segments.Add(new LineSegment(new Point(0, scaledY), false));
                        figure.Segments.Add(new LineSegment(new Point(GraphSettings.Width, scaledY), true));
                    }
                    figures.Add(figure);
                }
            }

            return figures;
        }

        private double findInterval(double dataSize, double tickCount = 0)
        {
            if (tickCount == 0)
            {
                tickCount = 0.3 * Math.Sqrt(dataSize);
            }

            double interval;
            double delta = dataSize / tickCount;
            double dec = -Math.Floor(Math.Log10(delta));
            double mag = Math.Pow(10, -dec);
            double norm = delta / mag;

            if (norm < 1.5)
            {
                interval = 1;
            }
            else if (norm < 3)
            {
                interval = 2;
                if (norm > 2.25)
                {
                    interval = 2.5;
                }
            }
            else if (norm < 7.5)
            {
                interval = 5;
            }
            else
            {
                interval = 10;
            }

            interval *= mag;

            return interval;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
