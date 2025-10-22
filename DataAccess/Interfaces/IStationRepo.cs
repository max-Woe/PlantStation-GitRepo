using DataAccess.Models;

namespace DataAccess.Interfaces
{
    /// <summary>
    /// Defines the repository contract for accessing and managing <see cref="Station"/> data.
    /// It extends <see cref="IRepo{T}"/> and adds station-specific query and management operations.
    /// </summary>
    public interface IStationRepo : IRepo<Station>
    {
        // ------------------------------------
        // READ Operations
        // ------------------------------------

        /// <summary>
        /// Retrieves a single station based on its unique MAC address.
        /// </summary>
        /// <param name="macAdress">The MAC address used to identify the station.</param>
        /// <returns>A Task that returns the found <see cref="Station"/> or <c>null</c> if no station matches the address.</returns>
        Task<Station?> GetByMacAdress(string macAdress);

        /// <summary>
        /// Retrieves all stations from the database. (Overrides the base <see cref="IRepo{T}"/> method if necessary).
        /// </summary>
        /// <returns>A Task that returns a <see cref="List{T}"/> of all <see cref="Station"/> objects. Returns an empty list on failure.</returns>
        new Task<List<Station>> GetAll();

        /// <summary>
        /// Retrieves the IDs of all stations currently in the database.
        /// </summary>
        /// <returns>A Task that returns a <see cref="List{T}"/> of all station IDs (<c>int</c>).</returns>
        Task<List<int>> GetAllIds();

        // ------------------------------------
        // UPDATE Operations
        // ------------------------------------

        /// <summary>
        /// Updates a list of existing stations in the database.
        /// This method is an alias for <see cref="IRepo{T}.UpdateByList(List{T})"/> specifically for stations.
        /// </summary>
        /// <param name="stations">The list of <see cref="Station"/> objects with updated values.</param>
        /// <returns>A Task that returns a list of the successfully updated <see cref="Station"/> objects.</returns>
        Task<List<Station>> UpdateStations(List<Station> stations);

        // ------------------------------------
        // DELETE Operations
        // ------------------------------------

        /// <summary>
        /// Deletes all stations from the database.
        /// This method is an alias for <see cref="IRepo{T}.DeleteAll()"/> specifically for stations.
        /// </summary>
        /// <returns>A Task that returns a <see cref="List{T}"/> of all deleted <see cref="Station"/> objects, or <c>null</c> if the operation fails.</returns>
        Task<List<Station>>? DeleteAllStations();
    }
}