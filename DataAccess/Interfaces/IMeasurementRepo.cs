using DataAccess.Models;
using DataAccess.Repositories;
using LoggingService;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace DataAccess.Interfaces
{
    /// <summary>
    /// Defines the contract for the repository responsible for managing and retrieving <see cref="Measurement"/> data.
    /// Extends <see cref="IRepo{T}"/> for generic CRUD operations.
    /// </summary>
    public interface IMeasurementRepo : IRepo<Measurement>
    {
        /// <summary>
        /// Creates a new measurement in the database.
        /// Optionally ensures the existence of the associated sensor if a MAC address is provided.
        /// </summary>
        /// <param name="measurement">The <see cref="Measurement"/> object to store.</param>
        /// <param name="macAddress">Optional MAC address of the sensor. Required to create a new sensor if it does not yet exist.</param>
        /// <returns>A Task that returns the stored measurement (<see cref="Measurement"/>) or <c>null</c> if saving fails or <paramref name="measurement"/> is invalid.</returns>
        Task<Measurement?> Create(Measurement measurement, string? macAddress = null);

        /// <summary>
        /// Retrieves all existing measurements from the database.
        /// </summary>
        /// <returns>A Task that returns a <see cref="List{T}"/> of all <see cref="Measurement"/> objects. Returns an empty list on error.</returns>
        Task<List<Measurement>> GetAllAsList();

        /// <summary>
        /// Retrieves all measurements belonging to a specific sensor ID.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose measurements are to be retrieved.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of <see cref="Measurement"/> objects for the specified sensor, or <c>null</c> on error.</returns>
        Task<List<Measurement>?> GetAllBySensorIdAsList(int sensorId);

        /// <summary>
        /// Deletes all measurements belonging to a specific sensor ID.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose measurements are to be deleted.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of the actually deleted <see cref="Measurement"/> objects. Returns an empty list on error or if none were found.</returns>
        Task<List<Measurement>> DeleteMeasurmentsBySensorId(int sensorId);

        /// <summary>
        /// Ensures that the sensor associated with the measurement exists in the database and links it to a station if necessary.
        /// The sensor is created if it does not exist and a MAC address is provided.
        /// </summary>
        /// <param name="measurement">The measurement whose sensor should be checked or created.</param>
        /// <param name="macAddress">The MAC address of the sensor/station. Can be <c>null</c> to only check for existence.</param>
        /// <returns>A Task that returns the existing or newly created <see cref="Sensor"/> object, or <c>null</c> on error or if no MAC address was provided for creation.</returns>
        Task<Sensor?> EnsureSensorExisting(Measurement measurement, string? macAddress);

        /// <summary>
        /// Retrieves the last <paramref name="count"/> measurements for a specific sensor.
        /// Measurements are ordered descending by their recording time.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor.</param>
        /// <param name="count">The maximum number of measurements to retrieve.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of the latest <see cref="Measurement"/> objects for the sensor. Returns an empty list on error or if <paramref name="count"/> is less than or equal to zero.</returns>
        Task<List<Measurement>> GetLastOfSensor(int sensorId, int count);

        /// <summary>
        /// Retrieves all measurements for a specific sensor recorded since a given point in time (<paramref name="since"/>).
        /// Measurements are ordered descending by their recording time.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor.</param>
        /// <param name="since">The point in time from which measurements should be retrieved.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of the sensor's <see cref="Measurement"/> objects recorded since the specified time. Returns an empty list on error.</returns>
        Task<List<Measurement>> GetLastOfSensorSince(int sensorId, DateTime since);
    }
}