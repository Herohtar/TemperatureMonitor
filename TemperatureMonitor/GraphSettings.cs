using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureMonitor
{
    public static class GraphSettings
    {
        public static double Width { get; set; }
        public static double Height { get; set; }
        public static DateTime Start { get; set; }
        public static DateTime End { get; set; }
        public static int Max { get; set; }
        public static int Min { get; set; }
        public static int Buffer { get; set; }
    }
}
