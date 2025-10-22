using DataAccess.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;

namespace ConverterService
{
    public class EntityConverter
    {
        static public bool IsValidJson(string jsonString)
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

        //static public async Task<List<Measurement>> ConvertStringToListOfMeasurements(string measurementsString)
        //{
        //    if (!IsValidJson(measurementsString))
        //    {
        //        return new List<Measurement>();
        //    }
        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //    };

        //    // Deserialisieren Sie die Liste mit den Optionen
        //    List<Measurement> measurements = JsonSerializer.Deserialize<List<Measurement>>(
        //        measurementsString,
        //        options)!;


        //    return measurements!;
        //}

        //static public async Task<List<Station>> ConvertStringToListOfStations(string stationString)
        //{
        //    if (!IsValidJson(stationString))
        //    {
        //        return new List<Station>();
        //    }
        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //    };

        //    // Deserialisieren Sie die Liste mit den Optionen
        //    List<Station> stations = JsonSerializer.Deserialize<List<Station>>(
        //        stationString,
        //        options)!;


        //    return stations!;
        //}

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

            // Deserialisieren Sie die Liste mit den Optionen
            List<T> stations = JsonSerializer.Deserialize<List<T>>(
                entityString,
                options)!;


            return stations!;
        }


    }
}
