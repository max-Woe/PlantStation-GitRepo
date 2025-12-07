using DataAccess.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal; // Not used, but kept for context
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Interfaces;

namespace ConverterService
{
    /// <summary>
    /// Provides generic static utility methods for converting JSON strings to lists of entities 
    /// and for validating JSON string format.
    /// </summary>
    public static class EntityConverter
    {
        /// <summary>
        /// Determines whether the specified string is a valid JSON document.
        /// </summary>
        /// <param name="jsonString">The string to be checked.</param>
        /// <returns><see langword="true"/> if the string is a valid JSON document; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// Uses <see cref="JsonDocument.Parse(string)"/> internally and catches a <see cref="JsonException"/> 
        /// if the string is not valid JSON. Returns <see langword="false"/> for null or whitespace strings.
        /// </remarks>
        private static bool IsValidJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return false;
            }
            try
            {
                JsonDocument.Parse(jsonString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static bool IsValid(string entityString)
        {
            if (string.IsNullOrEmpty(entityString))
            {
                return false;
            }

            try
            {
                using (JsonDocument.Parse(entityString))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        
        public static string ToJson(this IJsonSerializable obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Asynchronously converts a valid JSON string representation of a list of entities into a 
        /// generic <see cref="List{T}"/> of the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity objects to deserialize into (e.g., <c>Station</c>).</typeparam>
        /// <param name="entityString">The valid JSON string containing the array of entity data (e.g., "[{...}, {...}]").</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. 
        /// The task result contains a <see cref="List{T}"/> of type <typeparamref name="T"/>. 
        /// Returns an empty list if the input string is not valid JSON.</returns>
        /// <remarks>
        /// This method configures <see cref="JsonSerializerOptions"/> to use <see cref="JsonNamingPolicy.CamelCase"/> 
        /// for property naming matching during deserialization.
        /// **Note:** This method uses the null-forgiving operator (`!`) which assumes that 
        /// <see cref="JsonSerializer.Deserialize{TValue}(string, JsonSerializerOptions?)"/> will not return null 
        /// for a valid JSON string that can be deserialized into <c>List&lt;T&gt;</c>.
        /// </remarks>
        static public async Task<List<T>> ConvertStringToListOfEntities<T>(string entityString)
        {
            if (!IsValidJson(entityString))
            {
                return new List<T>();
            }
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            List<T> stations = JsonSerializer.Deserialize<List<T>>(
                entityString,
                options)!;


            return stations!;
        }
    }
}