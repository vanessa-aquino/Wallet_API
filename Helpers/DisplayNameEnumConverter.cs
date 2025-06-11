using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WalletAPI.Helpers
{
    public class DisplayNameEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var input = reader.GetString();

            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null && displayAttr.Name == input)
                    return (T)field.GetValue(null);

                if (field.Name == input)
                    return (T)field.GetValue(null);
            }

            throw new JsonException($"Unable to convert \"{input}\"to enum{typeof(T).Name}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var field = typeof(T).GetField(value.ToString());
            var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
            var displayName = displayAttr?.Name ?? value.ToString();

            writer.WriteStringValue(displayName);
        }
    }
}
