using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using LoggingService;

namespace DataAccess.Repositories 
{
    /// <summary>
    /// Implements the <see cref="IRepo{T}"/> and <see cref="IStationRepo"/> contracts for the <see cref="Station"/> entity.
    /// Provides concrete database access (CRUD) and logging functionality for Station entities.
    /// </summary>
    public class StationRepo(ApiContext context, ILoggingService logger) : BaseRepo(context, logger), IRepo<Station>, IStationRepo
    {
        // ------------------------------------
        // C - CREATE Operations
        // ------------------------------------

        /// <inheritdoc/>
        public async Task<Station> Create(Station station)
        {
            _logger.StartTimer();

            await TryExecuteAsync(async() => await _context.Stations.AddAsync(station), "AddAsync", "Create", station);
            await TryExecuteAsync(async() => await _context.SaveChangesAsync(), "SaveChangesAsync", "Create", station);
            
            _logger.StopTimer();
            
            return station;
        }
        /// <inheritdoc/>
        public async Task<List<Station>> CreateByList(List<Station> stations)
        {
            if(!stations.IsNullOrEmpty())
            {
                _logger.StartTimer();
                try
                {
                    foreach (var station in stations)
                    {
                        await TryExecuteAsync(async() => await _context.Stations.AddAsync(station), "AddAsync", "CreateByList", station);
                    }

                    await TryExecuteAsync(async() => await _context.SaveChangesAsync(), "SaveChangesAsync", "CreateByList", stations);
                }
                catch (Exception)
                {
                    return new List<Station>();
                }
                finally
                {
                    _logger.StopTimer();
                }
            }        

            return stations;
        }

        // ------------------------------------
        // R - READ Operations
        // ------------------------------------

        /// <inheritdoc/>
        public async Task<Station>? GetById(int id)
        {
            _logger.StartTimer();

            Station stationFromContext;

            try
            {
                stationFromContext = await TryExecuteAsync(async() => await _context.Stations.FindAsync(id), "FindAsync", "GetById", id);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }

            return stationFromContext;
        }
        /// <summary>
        /// Retrieves a single station entity based on its MAC address.
        /// </summary>
        /// <param name="macAdress">The MAC address of the station.</param>
        /// <returns>A Task that returns the found <see cref="Station"/> entity, or <c>null</c> if no matching station is found or on failure.</returns>
        public async Task<Station?> GetByMacAdress(string macAdress)
        {
            _logger.StartTimer();
            try
            {
                Station? stationFromContext = await TryExecuteAsync(async () => await _context.Stations.FirstOrDefaultAsync(s => s.MacAddress == macAdress), "FirstOrDefaultAsync", "GetByMacAdress", macAdress);

                if (stationFromContext == null)
                {
                    return null;
                }

                return stationFromContext;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Retrieves a list of station entities based on a list of their unique IDs.
        /// </summary>
        /// <param name="ids">The list of station IDs to retrieve.</param>
        /// <returns>A Task that returns a list of found <see cref="Station"/> entities. Returns an empty list if no matching stations are found or on failure.</returns>
        public async Task<List<Station>> GetByListOfIds(List<int> ids)
        {
            List<Station> stationsFromContext;
            Station? stationFromContext;

            _logger.StartTimer();

            try
            {
                stationsFromContext = new List<Station>();

                foreach (int id in ids)
                {
                    stationFromContext = await TryExecuteAsync(async() => await _context.Stations.FindAsync(id), "FindAsync", "GetByListOfIds", id);
                    if (stationFromContext != null)
                    {
                        stationsFromContext.Add(stationFromContext);
                    }
                }

                return stationsFromContext;
            }
            catch (Exception)
            {
                return new List<Station>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<List<Station>> GetAll()
        {
            _logger.StartTimer();
            try
            {
                List<Station>? stationsFromContext = new List<Station>();
                
                stationsFromContext = await TryExecuteAsync(async() => await _context.Stations.ToListAsync(), "ToListAsync", "GetAll", null);

                if (stationsFromContext == null)
                {
                    return new List<Station>();
                }

                return stationsFromContext;
            }
            catch (Exception)
            {
                return new List<Station>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Retrieves a list of all station IDs present in the database.
        /// </summary>
        /// <returns>A Task that returns a list of all station IDs (<see cref="int"/>). Returns an empty list on failure.</returns>
        public async Task<List<int>> GetAllIds()
        {

            _logger.StartTimer();

            try
            {
                List<int>? stationIdsFromContext = await TryExecuteAsync(async() => await _context.Stations.Select(s => s.Id).ToListAsync(), "ToListAsync", "GetAllIds", null);
                if (stationIdsFromContext == null)
                {
                    return new List<int>();
                }
                return stationIdsFromContext;
            }
            catch (Exception)
            {
                return new List<int>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }

        // ------------------------------------
        // U - UPDATE Operations
        // ------------------------------------

        /// <inheritdoc/>
        public async Task<Station?> Update(Station station)
        {
            _logger.StartTimer();

            try
            {
                Station? stationFromContext = await TryExecuteAsync(async () => await _context.Stations.FindAsync(station.Id), "FindAsync", "Update", null);

                if (stationFromContext == null)
                {
                    return null;
                }
                
                stationFromContext.Update(station);

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Update", stationFromContext);
                
                return stationFromContext;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Updates a list of station entities in the database.
        /// This method is functionally identical to <see cref="UpdateByList(List{Station})"/>.
        /// </summary>
        /// <param name="stations">The list of <see cref="Station"/> entities with updated data.</param>
        /// <returns>A Task that returns a list of the successfully updated <see cref="Station"/> entities. Returns an empty list if the input is empty or on failure.</returns>
        public async Task<List<Station>> UpdateStations(List<Station> stations)
        {
            if (stations.IsNullOrEmpty())
            {
                return new List<Station>();
            }

            _logger.StartTimer();

            Station? stationFromContext;
            List<Station> stationsFromContext = new List<Station>();
            List<int> stationIds = stations.Select(s => s.Id).ToList();

            try
            {
                stationsFromContext = await TryExecuteAsync(
                    async () => await _context.Stations.Where(s => stationIds.Contains(s.Id)).ToListAsync(), 
                    "ToListAsync", 
                    "UpdateStations", 
                    null);

                if(stationsFromContext.IsNullOrEmpty())
                {
                    return new List<Station>();
                }

                Dictionary<int,Station> stationDict = stations.ToDictionary(s => s.Id, s=>s);

                foreach (Station station in stationsFromContext)
                {
                    if(stationDict.TryGetValue(station.Id, out Station? updatedStation))
                    {
                        station.Update(updatedStation);
                    }
                }

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "UpdateStations", stationsFromContext);
                
                return stationsFromContext;
            }
            catch (Exception)
            {
                return new List<Station>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<List<Station>> UpdateByList(List<Station> stations)
        {
            if (stations.IsNullOrEmpty())
            {
                return new List<Station>();
            }

            _logger.StartTimer();

            try
            {
                Station? stationFromContext;
                List<Station> stationsFromContext = new List<Station>();
                foreach (Station station in stations)
                {
                    stationFromContext = await TryExecuteAsync(async () => await _context.Stations.FindAsync(station.Id), "FindAsync", "UpdateStations", station);
                    if (stationFromContext != null)
                    {
                        stationFromContext.Update(station);
                        stationsFromContext.Add(stationFromContext);
                    }
                }
                if (stationsFromContext.Count > 0)
                {
                    await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "UpdateStations", stationsFromContext);
                }
                return stationsFromContext;
            }
            catch (Exception)
            {
                return new List<Station>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }

        // ------------------------------------
        // D - DELETE Operations
        // ------------------------------------

        /// <inheritdoc/>
        public async Task<Station?> Delete(int id)
        {
            if(id <= 0)
            {
                return null;
            }

            _logger.StartTimer();

            try
            {
                Station? stationFromContext = await TryExecuteAsync(async () => await _context.Stations.FindAsync(id), "FindAsync", "Delete", id);
                if (stationFromContext == null)
                {
                    return null;
                }
                _context.Stations.Remove(stationFromContext);
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Delete", stationFromContext);

                return stationFromContext;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Deletes all station entities from the database.
        /// This method is functionally identical to <see cref="DeleteAll()"/>.
        /// </summary>
        /// <returns>A Task that returns a list of the deleted <see cref="Station"/> entities. Returns <c>null</c> or an empty list on failure or if no stations were present.</returns>
        public async Task<List<Station>>? DeleteAllStations()
        {
            _logger.StartTimer();
            
            try{
                List<Station> stationsFromContext = await TryExecuteAsync(async () => await _context.Stations.ToListAsync(), "ToListAsync", "DeleteAllStations", null);
                if (stationsFromContext == null || stationsFromContext.Count == 0)
                {
                    return new List<Station>();
                }

                await TryExecuteAsync<object>(() =>
                {
                    _context.Stations.RemoveRange(stationsFromContext);
                    return Task.FromResult<object>(null);
                }, "RemoveRange", "DeleteByListOfIds");

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "DeleteAllStations", stationsFromContext);
                
                return stationsFromContext;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<List<Station>> DeleteByListOfIds(List<int> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return new List<Station>();
            }

            _logger.StartTimer();

            try
            {
                List<Station> stationsFromContext = new List<Station>();
                foreach (int id in ids)
                {
                    stationsFromContext.AddRange(await TryExecuteAsync(async () => await _context.Stations.Where(s => s.Id == id).ToListAsync(), "Where.ToListAsync", "DeleteByListOfIds", id));
                }
                if (stationsFromContext.Count == 0)
                {
                    return new List<Station>();
                }

                await TryExecuteAsync<object>(() =>
                {
                    _context.Stations.RemoveRange(stationsFromContext);
                    return Task.FromResult<object>(null);
                }, "RemoveRange", "DeleteByListOfIds");

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "DeleteByListOfIds", stationsFromContext);
                return stationsFromContext;
            }
            catch (Exception)
            {
                return new List<Station>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<List<Station>> DeleteAll()
        {
            _logger.StartTimer();

            try
            {
                List<Station>? stationsFromContext = await TryExecuteAsync(async () => await _context.Stations.ToListAsync(), "ToListAsync", "DeleteAll", null);
                if (stationsFromContext == null || stationsFromContext.Count == 0)
                {
                    return new List<Station>();
                }

                await TryExecuteAsync<object>(() =>
                {
                    _context.Stations.RemoveRange(stationsFromContext);
                    return Task.FromResult<object>(null);
                }, "RemoveRange", "DeleteByListOfIds");

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "DeleteAll", stationsFromContext);
                return stationsFromContext;
            }
            catch (Exception)
            {
                return new List<Station>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }

    }
}
