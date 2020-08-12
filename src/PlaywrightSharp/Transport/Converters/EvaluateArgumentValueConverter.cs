using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlaywrightSharp.Helpers;
using PlaywrightSharp.Transport.Channels;

namespace PlaywrightSharp.Transport.Converters
{
    internal class EvaluateArgumentValueConverter<T> : JsonConverter<T>
    {
        private readonly EvaluateArgument _parentObject;

        public EvaluateArgumentValueConverter()
        {
        }

        internal EvaluateArgumentValueConverter(EvaluateArgument parentObject)
        {
            _parentObject = parentObject;
        }

        public override bool CanConvert(Type type) => true;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            var result = document.RootElement;

            return (T)ParseEvaluateResult(result, typeof(T), options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteStartObject();
                writer.WriteNull("v");
                writer.WriteEndObject();

                return;
            }

            if (value is double nan && double.IsNaN(nan))
            {
                writer.WriteStartObject();
                writer.WriteString("v", "NaN");
                writer.WriteEndObject();

                return;
            }

            if (value is double infinity && double.IsPositiveInfinity(infinity))
            {
                writer.WriteStartObject();
                writer.WriteString("v", "Infinity");
                writer.WriteEndObject();

                return;
            }

            if (value is double negativeInfinity && double.IsNegativeInfinity(negativeInfinity))
            {
                writer.WriteStartObject();
                writer.WriteString("v", "-Infinity");
                writer.WriteEndObject();

                return;
            }

            if (value is double negativeZero && negativeZero.IsNegativeZero())
            {
                writer.WriteStartObject();
                writer.WriteString("v", "-0");
                writer.WriteEndObject();

                return;
            }

            if (IsPrimitiveValue(value.GetType()))
            {
                JsonSerializer.Serialize(writer, value);
                return;
            }

            if (value is DateTime date)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("d");
                JsonSerializer.Serialize(writer, date);
                writer.WriteEndObject();

                return;
            }

            if (value is IDictionary)
            {
                JsonSerializer.Serialize(writer, value);
                return;
            }

            if (value is IEnumerable array)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("a");
                writer.WriteStartArray();

                foreach (object item in array)
                {
                    JsonSerializer.Serialize(writer, item, options);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();

                return;
            }

            if (value is IChannelOwner channelOwner)
            {
                _parentObject.Guids.Add(new EvaluateArgumentGuidElement { Guid = channelOwner.Channel.Guid });

                writer.WriteStartObject();
                writer.WriteNumber("h", _parentObject.Guids.Count - 1);
                writer.WriteEndObject();

                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("o");
            writer.WriteStartObject();

            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(value))
            {
                object obj = propertyDescriptor.GetValue(value);
                writer.WritePropertyName(propertyDescriptor.Name);

                if (obj == null)
                {
                    writer.WriteStartObject();
                    writer.WriteNull("v");
                    writer.WriteEndObject();
                }
                else
                {
                    JsonSerializer.Serialize(writer, obj, options);
                }
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        private static bool IsPrimitiveValue(Type type)
            => type == typeof(string) ||
            type == typeof(int) ||
            type == typeof(decimal) ||
            type == typeof(double) ||
            type == typeof(bool) ||
            type == typeof(int?) ||
            type == typeof(decimal?) ||
            type == typeof(double?) ||
            type == typeof(bool?);

        private static object ParseEvaluateResult(JsonElement result, Type t, JsonSerializerOptions options)
        {
            if (result.ValueKind == JsonValueKind.Object && result.TryGetProperty("v", out var value))
            {
                if (value.ValueKind == JsonValueKind.Null)
                {
                    return GetDefaultValue(t);
                }

                return value.ToString() switch
                {
                    "undefined" => GetDefaultValue(t),
                    "Infinity" => double.PositiveInfinity,
                    "-Infinity" => double.NegativeInfinity,
                    "-0" => -0d,
                    "NaN" => double.NaN,
                    _ => value.ToObject(t),
                };
            }

            if (result.ValueKind == JsonValueKind.Object && result.TryGetProperty("d", out var date))
            {
                return date.ToObject<DateTime>();
            }

            if (result.ValueKind == JsonValueKind.Object && result.TryGetProperty("o", out var obj))
            {
                if (t == typeof(ExpandoObject) || t == typeof(object))
                {
                    return ReadObject(obj, options);
                }

                return obj.ToObject(t);
            }

            if (result.ValueKind == JsonValueKind.Object && result.TryGetProperty("v", out var vNull) && vNull.ValueKind == JsonValueKind.Null)
            {
                return GetDefaultValue(t);
            }

            if (result.ValueKind == JsonValueKind.Object && result.TryGetProperty("a", out var array) && array.ValueKind == JsonValueKind.Array)
            {
                if (t == typeof(ExpandoObject) || t == typeof(object))
                {
                    return ReadList(array, options);
                }

                return array.ToObject(t, options);
            }

            if (t == typeof(JsonElement?))
            {
                return result;
            }

            if (result.ValueKind == JsonValueKind.Array)
            {
                var serializerOptions = JsonExtensions.GetNewDefaultSerializerOptions(false);
                serializerOptions.Converters.Add(GetNewConverter(t.GetElementType()));

                var resultArray = new ArrayList();
                foreach (var item in result.EnumerateArray())
                {
                    resultArray.Add(ParseEvaluateResult(item, t.GetElementType(), serializerOptions));
                }

                var destinationArray = Array.CreateInstance(t.GetElementType(), resultArray.Count);
                Array.Copy(resultArray.ToArray(), destinationArray, resultArray.Count);

                return destinationArray;
            }

            return result.ToObject(t);
        }

        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }

            return null;
        }

        private static JsonConverter GetNewConverter(Type type)
        {
            var converter = typeof(EvaluateArgumentValueConverter<>);
            Type[] typeArgs = { type };
            var makeme = converter.MakeGenericType(typeArgs);
            return (JsonConverter)Activator.CreateInstance(makeme);
        }

        private static Type ValueKindToType(JsonElement element)
            => element.ValueKind switch
            {
                JsonValueKind.Array => typeof(Array),
                JsonValueKind.String => typeof(string),
                JsonValueKind.Number => decimal.Truncate(element.ToObject<decimal>()) != element.ToObject<decimal>() ? typeof(decimal) : typeof(int),
                JsonValueKind.True => typeof(bool),
                JsonValueKind.False => typeof(bool),
                _ => typeof(object),
            };

        private static object ReadList(JsonElement jsonElement, JsonSerializerOptions options)
        {
            IList<object> list = new List<object>();
            foreach (var item in jsonElement.EnumerateArray())
            {
                list.Add(ParseEvaluateResult(item, ValueKindToType(item), options));
            }

            return list.Count == 0 ? null : list;
        }

        private static object ReadObject(JsonElement jsonElement, JsonSerializerOptions options)
        {
            IDictionary<string, object> expandoObject = new ExpandoObject();
            foreach (var obj in jsonElement.EnumerateObject())
            {
                expandoObject[obj.Name] = ParseEvaluateResult(obj.Value, ValueKindToType(obj.Value), options);
            }

            return expandoObject;
        }
    }
}
