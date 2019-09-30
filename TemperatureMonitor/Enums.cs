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

    public enum SensorType
    {
        Thermostat,
        RemoteTemperatureSensor
    }
}
