namespace UptimeMonitor.Monitors.HttpMonitor
{
    public class ResponseTimeMeasurementEnumConverter : DefaultFallbackStringEnumConverter<ResponseTimeMeasurement>
    {
        protected override ResponseTimeMeasurement GetDefaultValue() => ResponseTimeMeasurement.Request;
        public override bool CanConvert(Type typeToConvert) => base.CanConvert(typeToConvert) && typeToConvert == typeof(ResponseTimeMeasurement);
    }
}