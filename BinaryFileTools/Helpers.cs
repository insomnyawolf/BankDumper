using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinaryFileTools
{
    // You can't easily read/write byte[] without a custom converter like thisone
    internal class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var charArray = JsonSerializer.Deserialize<uint[]>(ref reader);

            if (charArray == null)
            {
                return null;
            }

            return Unsafe.As<byte[]>(charArray);
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            for (int i = 0; i < value.Length; i++)
            {
                writer.WriteNumberValue(value[i]);
            }

            writer.WriteEndArray();
        }
    }

    public abstract class JsonSerializable
    {
        internal static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions);
        }

        public string ToString(object value)
        {
            return JsonSerializer.Serialize(value, JsonSerializerOptions);
        }
    }

    internal static class SortHelper
    {
        public static void SelectionSort<ItemType>(IList<ItemType> list, Func<ItemType, ItemType, bool> condition)
        {
            var currentOrderIndex = list.Count;

            // do till everything is in order
            while (currentOrderIndex > 0)
            {
                int tempIndex = 0;

                // Don't check already ordered patterns
                for (int listIndex = 0; listIndex < currentOrderIndex; listIndex++)
                {
                    if (!condition(list[listIndex], list[tempIndex]))
                    {
                        tempIndex = listIndex;
                    }
                }

                // At least the last number should be ordered so you don't need to check it again
                currentOrderIndex--;

                // Prevents extra work when the selected position and the position being ordered are already in order
                if (tempIndex < currentOrderIndex)
                {
                    var tempValue = list[tempIndex];
                    list[tempIndex] = list[currentOrderIndex];
                    list[currentOrderIndex] = tempValue;
                }
            }
        }
    }
}
