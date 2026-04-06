using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrdersDownloader.Models
{
    public class OrderItem
    {
        public long id { get; set; }
        public string name { get; set; }
        [JsonPropertyName("price")]
        [JsonConverter(typeof(PriceConverter))]
        public decimal? price { get; set; }

        [JsonPropertyName("quantity")]
        [JsonConverter(typeof(QuantityConverter))]
        public int quantity { get; set; }
        public string currency { get; set; }
        public string measure_unit { get; set; }
        public string image { get; set; }
        public string url { get; set; }
        public string external_id { get; set; }
    }

    public class PriceConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value)) return 0;

            var match = Regex.Match(value, @"[\d,.]+");
            if (match.Success)
            {
                string num = match.Value.Replace(",", ".");
                if (decimal.TryParse(num, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    return result;
            }
            return 0;
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // Записываем с 2 знаками после запятой
            writer.WriteStringValue(value.ToString("F2", CultureInfo.InvariantCulture));
        }
    }
    public class IntFromDoubleConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int i))
                    return i;

                // Если число с плавающей точкой
                if (reader.TryGetDouble(out double d))
                    return (int)d;
            }

            throw new JsonException("Cannot convert to int");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class QuantityConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int i)) return i;
                if (reader.TryGetDouble(out double d)) return (int)d;
            }
            throw new JsonException("Cannot convert to int");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            // Для XML лучше писать с 2 знаками после запятой
            writer.WriteStringValue(((decimal)value).ToString("F2", CultureInfo.InvariantCulture));
        }
    }
}