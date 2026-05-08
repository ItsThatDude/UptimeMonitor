using System.Text.Json;
using System.Text.Json.Serialization;

namespace UptimeMonitor.Monitors.HttpMonitor
{
    public class DefaultFallbackStringEnumConverter<T> : JsonConverterFactoryDecorator where T : struct, Enum
    {
        public DefaultFallbackStringEnumConverter(JsonStringEnumConverter inner) : base(inner) { }
        public DefaultFallbackStringEnumConverter() : this(new JsonStringEnumConverter()) { }

        protected virtual T GetDefaultValue() => default(T);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var targetType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            var inner = base.CreateConverter(targetType, options);
            return (JsonConverter?)Activator.CreateInstance(typeof(EnumConverterDecorator), new object?[] { this, inner });
        }

        sealed class EnumConverterDecorator : JsonConverter<T>
        {
            readonly DefaultFallbackStringEnumConverter<T> parent;
            readonly JsonConverter<T> inner;
            public EnumConverterDecorator(DefaultFallbackStringEnumConverter<T> parent, JsonConverter inner) =>
                (this.parent, this.inner) = (parent ?? throw new ArgumentException(nameof(parent)), (inner as JsonConverter<T>) ?? throw new ArgumentException(nameof(inner)));

            public override bool CanConvert(Type typeToConvert) => inner.CanConvert(typeToConvert);

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    return inner.Read(ref reader, typeToConvert, options);
                }
                catch (JsonException)
                {
                    return parent.GetDefaultValue();
                }
            }
            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => inner.Write(writer, value, options);
        }
    }

    public class JsonConverterFactoryDecorator : JsonConverterFactory
    {
        readonly JsonConverterFactory inner;
        public JsonConverterFactoryDecorator(JsonConverterFactory inner) => this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        public override bool CanConvert(Type typeToConvert) => inner.CanConvert(typeToConvert);
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => inner.CreateConverter(typeToConvert, options);
    }
}