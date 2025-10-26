using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

namespace ConverterService
{
    /// <summary>
    /// Provides static methods for converting data related to <see cref="Station"/> objects.
    /// This class is static and cannot be instantiated.
    /// </summary>
    static public class StationConverter
    {
        /// <summary>
        /// Asynchronously converts a JSON string representation of a list of stations into a <see cref="List{T}"/> of <see cref="Station"/> objects.
        /// </summary>
        /// <param name="responseString">The JSON string containing the array of station data.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. 
        /// The task result contains a <see cref="List{T}"/> of <see cref="Station"/> objects. 
        /// Returns an empty list if the input string is null or empty, or if deserialization fails.</returns>
        static public async Task<List<Station>> ConvertStringToListOfStations(string responseString)
        {
            List<Station> stations = new List<Station>();
            if (string.IsNullOrEmpty(responseString))
            {
                return stations;
            }
            try
            {
                stations = System.Text.Json.JsonSerializer.Deserialize<List<Station>>(responseString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler bei der Deserialisierung der Stationsdaten: {ex.Message}");
            }
            return stations ?? new List<Station>();
        }
    }
}
