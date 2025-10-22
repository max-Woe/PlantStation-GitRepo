using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using System.Net.Http;

using ConverterService;

namespace WPFClient.Models
{
    /// <summary>
    /// Represents the data required for plotting a time series of measurements for a specific sensor.
    /// Manages a list of <see cref="Measurement"/> objects and their update
    /// from an API.
    /// </summary>
    public class PlotData
    {
        private List<Measurement> _measurements = new List<Measurement>();
        private int _sensorId;
        private DateTime? _lastUpdate = null;
        private HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "http://192.168.178.72:7028/api/measurements"; 
        private ApiClient _apiClient = new ApiClient();

        /// <summary>
        /// Gets the list of measurements used for the plot.
        /// </summary>
        public List<Measurement> Measurements
        {
            get { return _measurements; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlotData"/> class with the specified sensor ID.
        /// </summary>
        /// <param name="sensorId">The unique identifier (ID) of the sensor whose data is to be plotted.</param>
        public PlotData(int sensorId)
        {
            _sensorId = sensorId;
        }

        /// <summary>
        /// Asynchronously creates and initializes a new instance of <see cref="PlotData"/>
        /// and fetches the initial measurements for the specified time span.
        /// </summary>
        /// <param name="sensorId">The unique identifier (ID) of the sensor.</param>
        /// <param name="timeSpan">The time span (in minutes) for which to fetch data.</param>
        /// <returns>A Task representing the created and data-filled <see cref="PlotData"/> object.</returns>
        public static async Task<PlotData> CreatePlot(int sensorId, int timeSpan)
        {
            var newPlotData = new PlotData(sensorId);
            await newPlotData.Update(timeSpan);
            return newPlotData;
        }

        /// <summary>
        /// Asynchronously updates the list of measurements by fetching new data from the API
        /// and appending it to the existing list.
        /// </summary>
        /// <remarks>
        /// On the first call (<see cref="Measurements"/> is empty), the latest measurements
        /// for the entire <paramref name="timeSpan"/> are retrieved. On subsequent calls,
        /// only measurements recorded since the time of <see cref="_lastUpdate"/> are fetched
        /// to update the data.
        /// </remarks>
        /// <param name="timeSpan">The time span (in minutes) for which initial data should be fetched.</param>
        /// <returns>A Task representing the update operation.</returns>
        public async Task Update(int timeSpan)
        {
            if(_measurements.Count == 0)
            {
                string? responseString = await _apiClient.GetMeasurementsFromApiAsyncAsString( "GetLastOfSensor", _sensorId, timeSpan);
                
                _measurements = await EntityConverter.ConvertStringToListOfEntities<Measurement>(responseString);
            }
            else
            {
                string? responseString = await _apiClient.GetMeasurementsFromApiAsyncAsString("GetLastOfSensorSince", _sensorId, timeSpan, _lastUpdate);
                List<Measurement> newMeasurements = await EntityConverter.ConvertStringToListOfEntities<Measurement>(responseString);
                _measurements.AddRange(newMeasurements);
            }

            if (_measurements.Count > 0)
            {
                _lastUpdate = _measurements[_measurements.Count - 1].RecordedAt;
            }
        }
    }    
}
