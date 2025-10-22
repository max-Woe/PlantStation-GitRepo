using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    /// <summary>
    /// Represents a collection of measurements send by the sensor via http.
    /// </summary>
    public class MeasurementCollection
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [Key]
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets the water level measurement.
        /// </summary>
        public double WaterLevel { get; set; }


        /// <summary>
        /// Gets or sets the moisture measurement.
        /// </summary>
        public double Moisture { get; set; }


        /// <summary>
        /// Gets or sets the temperature measurement.
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Gets or sets the water level measurement.
        /// </summary>
        public double Humidity { get; set; }

        /// <summary>
        /// Gets or sets the stationId.
        /// </summary>
        public int StationId { get; set; }
        
        /// <summary>
        /// Gets or sets the MacAdress of the Board, representing the Station.
        /// </summary>
        public string MacAddress { get; set; }

        public override string ToString()
        {
            return $"Time = {Time}, WaterLevel = {WaterLevel}, Moisture = {Moisture}, Temperature = {Temperature}, Humidity = {Humidity}, StationId = {StationId}, MacAddress = {MacAddress}";
        }

    }
}
