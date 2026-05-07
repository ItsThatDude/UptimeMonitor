using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using UptimeMonitor.Core.Plugins.PluginBase.Monitoring;

namespace UptimeMonitor.Monitors.HttpMonitor
{
    public enum ResponseTimeMeasurement
    {
        [EnumMember(Value = "Request")]
        Request,
        [EnumMember(Value = "SslHandshake")]
        SslHandshake,
        [EnumMember(Value = "SocketConnect")]
        SocketConnect,
        [EnumMember(Value = "RequestHeaders")]
        RequestHeaders,
        [EnumMember(Value = "ResponseHeaders")]
        ResponseHeaders,
        [EnumMember(Value = "ResponseContent")]
        ResponseContent,
        [EnumMember(Value = "TimeToHeaders")]
        TimeToHeaders,
        [EnumMember(Value = "Internal")]
        Internal,
    }

    public class HttpMonitorSettings : IMonitorSettings<HttpMonitor>
    {
        [SettingDefinition(
            displayName:"HTTP Method", 
            dataType:SettingDataType.String, 
            required:true, 
            allowMultiple:false, 
            description:"The HTTP Method used to send the request."
        )]
        public string Method { get; set; } = "GET";

        [SettingDefinition(
            displayName:"Accepted Status Codes", 
            dataType:SettingDataType.Int, 
            required:true, 
            allowMultiple:true, 
            description:"The status codes expected that indicates success."
        )]
        [DefaultValue(new int[] {200, 301, 302})]
        public int[] AcceptedStatusCodes { get; set; } = { 200, 301, 302 };

        [SettingDefinition(
            displayName:"Response Time Measurement", 
            dataType:SettingDataType.String, 
            required:false, 
            allowMultiple:false,
            description:"The method used to measure response time."
        )]
        [DefaultValue(nameof(ResponseTimeMeasurement.Request))]
        [JsonConverter(typeof(ResponseTimeMeasurementEnumConverter))]
        public ResponseTimeMeasurement ResponseTimeMeasurement { get; set; } = ResponseTimeMeasurement.Request;

        [SettingDefinition(
            displayName:"Ignore Invalid Certificates", 
            dataType:SettingDataType.Boolean, 
            required:false, 
            allowMultiple:false,
            description:"Accept invalid certificates."
        )]
        public bool IgnoreInvalidCertificates { get; set; } = false;

        [SettingDefinition(
            displayName:"CA Certificate", 
            dataType:SettingDataType.MultilineString, 
            required:false, 
            allowMultiple:false, 
            description:"The CA Certificate used to validate the HTTPs Certificate."
        )]
        public string? CACertificate { get; set; } = null;
    }
}
