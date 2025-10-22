using DataAccess.Models;

namespace DataAccess.Interfaces
{
    /// <summary>
    /// Defines the repository contract for accessing and managing <see cref="Sensor"/> data.
    /// It extends <see cref="IRepo{T}"/> and adds sensor-specific query and management operations.
    /// </summary>
    public interface ISensorRepo : IRepo<Sensor>
    {

        // ------------------------------------
        // Overloaded CREATE Operation
        // ------------------------------------

        /// <summary>
        /// Creates a new sensor in the database.
        /// It optionally ensures the associated station exists, using the provided MAC address.
        /// </summary>
        /// <param name="sensor">The <see cref="Sensor"/> object to store.</param>
        /// <param name="macAdress">Optional MAC address of the station. Required to ensure the station exists and to link the sensor.</param>
        /// <returns>A Task that returns the stored <see cref="Sensor"/> object, or <c>null</c> if saving fails.</returns>
        new Task<Sensor> Create(Sensor sensor, string? macAdress = null);

        // ------------------------------------
        // READ Operations
        // ------------------------------------

        /// <summary>
        /// Retrieves all sensors belonging to a specific station ID.
        /// </summary>
        /// <param name="stationId">The ID of the station.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of <see cref="Sensor"/> objects for the specified station.</returns>
        Task<List<Sensor>> GetSensorsByStationId(int stationId);

        /// <summary>
        /// Retrieves a specific sensor based on the Station ID and the sensor type.
        /// </summary>
        /// <param name="stationId">The ID of the station the sensor belongs to.</param>
        /// <param name="type">The type of the sensor (e.g., "Temperature", "Humidity").</param>
        /// <returns>A Task that returns the matching <see cref="Sensor"/> or <c>null</c> if not found.</returns>
        Task<Sensor?> GetSensorsByStationIdAndType(int stationId, string type);

        /// <summary>
        /// Retrieves all sensors belonging to a specific station ID. This method is an alias for <see cref="GetSensorsByStationId(int)"/>.
        /// </summary>
        /// <param name="id">The ID of the station.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of <see cref="Sensor"/> objects for the specified station.</returns>
        Task<List<Sensor>> GetListByStationId(int id);

        /// <summary>
        /// Retrieves the IDs of all sensors currently in the database.
        /// </summary>
        /// <returns>A Task that returns a <see cref="List{T}"/> of all sensor IDs (<c>int</c>).</returns>
        Task<List<int>> GetAllIds();

        /// <summary>
        /// Retrieves the IDs of all sensors belonging to a specific station.
        /// </summary>
        /// <param name="stationId">The ID of the station.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of sensor IDs (<c>int</c>) for the specified station.</returns>
        Task<List<int>> GetIdsByStationId(int stationId);

        // ------------------------------------
        // DELETE Operations
        // ------------------------------------

        /// <summary>
        /// Deletes all sensors associated with a given station ID.
        /// </summary>
        /// <param name="stationId">The ID of the station whose sensors should be deleted.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of the deleted <see cref="Sensor"/> objects, or <c>null</c> if the operation fails.</returns>
        Task<List<Sensor>>? DeleteListByStationId(int stationId);

        /// <summary>
        /// Deletes a specific list of sensors from the database.
        /// </summary>
        /// <param name="sensors">The list of <see cref="Sensor"/> entities to be deleted.</param>
        /// <returns>A Task that returns a <see cref="List{T}"/> of the deleted <see cref="Sensor"/> objects, or <c>null</c> if the operation fails.</returns>
        Task<List<Sensor>>? DeleteList(List<Sensor> sensors);

        /// <summary>
        /// Deletes all sensors from the database. (Overrides the base <see cref="IRepo{T}"/> method if needed).
        /// </summary>
        /// <returns>A Task that returns a <see cref="List{T}"/> of all deleted <see cref="Sensor"/> objects.</returns>
        new Task<List<Sensor>> DeleteAll();

        // ------------------------------------
        // UTILITY Operation
        // ------------------------------------

        /// <summary>
        /// Ensures that the station associated with the sensor exists in the database.
        /// If the station is not found, a new station is created using the provided MAC address.
        /// </summary>
        /// <param name="sensor">The optional sensor entity used for context (e.g., to read StationId).</param>
        /// <param name="macAddress">The MAC address required to identify or create the station.</param>
        /// <returns>A Task that returns the existing or newly created <see cref="Station"/> object, or <c>null</c> if creation fails (e.g., if MAC address is missing).</returns>
        Task<Station?> EnsureStationExisting(Sensor? sensor, string? macAddress);
    }
}