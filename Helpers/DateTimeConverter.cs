using System.Text.Json;
using System.Text.Json.Serialization;

namespace WalletAPI.Helpers
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string _format = "dd/MM/yyyy HH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return DateTime.ParseExact(value, _format, null);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }
}
