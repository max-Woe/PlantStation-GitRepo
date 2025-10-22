using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    /// <summary>
    /// Represents a single measurement value received from an external source (e.g., a sensor device).
    /// This class serves as a Data Transfer Object (DTO) and contains specific logic for deserializing Unix timestamps.
    /// </summary>
    public class ReceivedMeasurement
    {
        /// <summary>
        /// The precise time (in UTC) when the measurement was recorded.
        /// This value is set indirectly via the <see cref="UnixTime"/> property.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// The Unix timestamp (seconds since 1970-01-01) used for deserialization.
        /// When set, the value is automatically converted and assigned to the <see cref="Time"/> property (as UTC).
        /// </summary>
        // The deserializer uses this property to set the Unix timestamp.
        public long UnixTime
        {
            set => Time = DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime;
        }

        /// <summary>
        /// The numerical value of the measurement (e.g., 21.5 for temperature).
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// The unit of the measurement (e.g., "°C", "hPa", "%").
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// The type of the measurement (e.g., "temperature", "pressure", "humidity").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The pin number of the sensor on the device (used for identifying the specific sensor).
        /// </summary>
        public int Pin { get; set; }

        /// <summary>
        /// The MAC address of the device or station that sent this measurement.
        /// Used for source identification.
        /// </summary>
        public string MacAddress { get; set; }

        /// <summary>
        /// Returns a string representation of the current measurement object.
        /// </summary>
        /// <returns>A string containing all important properties of the measurement.</returns>
        public override string ToString()
        {
            return $"Time = {Time}, Value = {Value}, Unit = {Unit}, Type = {Type}, Pin = {Pin}, MacAddress = {MacAddress}";
        }
    }
}