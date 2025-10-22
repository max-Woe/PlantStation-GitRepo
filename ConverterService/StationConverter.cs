using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

namespace ConverterService
{
    static public class StationConverter
    {
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
