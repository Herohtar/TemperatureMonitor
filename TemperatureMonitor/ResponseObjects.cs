using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureMonitor
{
    public class ResponseMessage
    {
    }

    public class ErrorResponseMessage
    {
    }

    public class TemperatureSensorsData
    {
        public int DeviceType { get; set; }
        public int RemoteTemperatureEnableStatus { get; set; }
        public bool IsActive { get; set; }
        public bool LowBattery { get; set; }
        public bool Malfunction { get; set; }
        public int? PairedThermostatId { get; set; }
        public bool PairedInManual { get; set; }
        public bool PairedInSchedule { get; set; }
        public int UnitId { get; set; }
        public int DeviceId { get; set; }
        public string Description { get; set; }
        public int ReadingType { get; set; }
        public int LastKnownReading { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string TimeStampTextString { get; set; }
        public bool HasReading { get; set; }
    }

    public class ResponseObject
    {
        public List<TemperatureSensorsData> temperatureSensorsData { get; set; }
    }

    public class D
    {
        public string __type { get; set; }
        public bool success { get; set; }
        public bool error { get; set; }
        public bool problem { get; set; }
        public List<object> successMessage { get; set; }
        public List<object> errorMessage { get; set; }
        public List<object> problemMessage { get; set; }
        public ResponseMessage responseMessage { get; set; }
        public ErrorResponseMessage errorResponseMessage { get; set; }
        public ResponseObject responseObject { get; set; }
        public bool HasError { get; set; }
    }

    public class RootObject
    {
        public D d { get; set; }
    }
}
