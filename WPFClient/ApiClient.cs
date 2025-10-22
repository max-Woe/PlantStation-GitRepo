using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

using ConverterService;

namespace WPFClient
{
    /// <summary>
    /// Provides methods to communicate with the PlantStation API, handling HTTP requests 
    /// and deserializing JSON responses.
    /// </summary>
    class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _httpHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClient"/> class.
        /// Configures the <see cref="HttpClient"/> with a base address and a handler
        /// that trusts all server certificates (disables SSL/TLS certificate validation
        /// for local development/testing).
        /// </summary>
        public ApiClient()
        {
            _httpHandler = new HttpClientHandler();
            _httpHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            _httpClient = new HttpClient(_httpHandler);
            _httpClient.BaseAddress = new Uri("https://localhost:7028/api");
        }

        /// <summary>
        /// Asynchronously retrieves measurement data from a specified API endpoint as a raw string.
        /// </summary>
        /// <param name="endpoint">The specific endpoint path within the Measurement controller (e.g., "GetRecent").</param>
        /// <param name="sensorId">The ID of the sensor for which to fetch measurements.</param>
        /// <param name="timeSpan">The number of measurements/time unit to retrieve (used if <paramref name="since"/> is null).</param>
        /// <param name="since">Optional parameter specifying the starting <see cref="DateTime"/> for data retrieval. If provided, overrides <paramref name="timeSpan"/>.</param>
        /// <returns>A Task that returns the raw JSON response body string, or <c>null</c> if an error occurs.</returns>
        public async Task<string?> GetMeasurementsFromApiAsyncAsString(string endpoint, int sensorId, int timeSpan, DateTime? since = null)
        {
            if (since == null)
            {
                endpoint += $"?sensorId={sensorId}&count={timeSpan}";
            }
            else
            {
                endpoint += $"?sensorId={sensorId}&since={since.Value.ToString("o")}";
            }

            try
            {
                string resourceEndpoint = $"api/Measurement/{endpoint}";

                HttpResponseMessage response = await _httpClient.GetAsync(resourceEndpoint);
                response.EnsureSuccessStatusCode(); 

                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Anfragefehler: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of all station entities from the API.
        /// </summary>
        /// <typeparam name="T">The expected type of the station entity (e.g., <c>Station</c>).</typeparam>
        /// <returns>A Task that returns a list of objects of type T, or an empty list if an error occurs.</returns>
        public async Task<List<T>> GetAllStationsFromApiAsync<T>()
        {

            try
            {
                string resourceEndpoint = $"api/Station/GetAll";

                HttpResponseMessage response = await _httpClient.GetAsync(resourceEndpoint);
                response.EnsureSuccessStatusCode(); 
                
                string responseBodyString = await response.Content.ReadAsStringAsync();
                List<T> responseBody = await EntityConverter.ConvertStringToListOfEntities<T>(responseBodyString);
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Anfragefehler: {e.Message}");
                return new List<T>();
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of all Station IDs from the API.
        /// </summary>
        /// <typeparam name="T">A placeholder type parameter .</typeparam>
        /// <returns>A Task that returns a list of integer Station IDs, or an empty list if an error occurs.</returns>
        public async Task<List<int>> GetAllStationIdsFromApiAsync<T>()
        {

            try
            {
                string resourceEndpoint = $"api/Station/GetIds";

                HttpResponseMessage response = await _httpClient.GetAsync(resourceEndpoint);
                response.EnsureSuccessStatusCode(); 
                
                string responseBodyString = await response.Content.ReadAsStringAsync();
                List<int> responseBody = await EntityConverter.ConvertStringToListOfEntities<int>(responseBodyString);

                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Anfragefehler: {e.Message}");
                return new List<int>();
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of all Sensor IDs from the API.
        /// </summary>
        /// <typeparam name="T">A placeholder type parameter.</typeparam>
        /// <returns>A Task that returns a list of integer Sensor IDs, or an empty list if an error occurs.</returns>
        public async Task<List<int>> GetAllSensorIdsFromApiAsync<T>()
        {

            try
            {
                string resourceEndpoint = $"api/Sensor/GetIds";

                HttpResponseMessage response = await _httpClient.GetAsync(resourceEndpoint);
                response.EnsureSuccessStatusCode(); 
                
                string responseBodyString = await response.Content.ReadAsStringAsync();
                List<int> responseBody = await EntityConverter.ConvertStringToListOfEntities<int>(responseBodyString);
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Anfragefehler: {e.Message}");
                return new List<int>();
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of Sensor IDs associated with a specific station ID from the API.
        /// </summary>
        /// <typeparam name="T">A placeholder type parameter.</typeparam>
        /// <param name="stationId">The ID of the station for which to retrieve sensor IDs.</param>
        /// <returns>A Task that returns a list of integer Sensor IDs, or an empty list if the station ID is invalid or an error occurs.</returns>
        public async Task<List<int>> GetAllSensorIdsByStationIdFromApiAsync<T>(int stationId)
        {

            string resourceEndpoint = $"api/Sensor/GetIdsByStationId";

            if (stationId <= 0)
            {
                return new List<int>();
            }
            else
            {
                resourceEndpoint += $"?stationId={stationId}";
            }

            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync(resourceEndpoint);
                response.EnsureSuccessStatusCode(); 
                
                string responseBodyString = await response.Content.ReadAsStringAsync();
                List<int> responseBody = await EntityConverter.ConvertStringToListOfEntities<int>(responseBodyString);
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Anfragefehler: {e.Message}");
                return new List<int>();
            }
        }
    }
}
