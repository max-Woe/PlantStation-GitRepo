using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;
using Serilog;
using LoggingService;
using Microsoft.IdentityModel.Tokens;

namespace DataAccess.Repositories
{
    /// <summary>
    /// Implements the <see cref="IRepo{T}"/> and <see cref="ISensorRepo"/> contracts for the <see cref="Sensor"/> entity.
    /// Provides concrete database access (CRUD) and logging functionality, including logic to ensure the associated <see cref="Station"/> exists.
    /// </summary>
    public class SensorRepo(ApiContext context, ILoggingService logger, IStationRepo stationRepo) : BaseRepo(context, logger), IRepo<Sensor>, ISensorRepo
    {
        private readonly IStationRepo _stationRepo = stationRepo;
        
        // ------------------------------------
        // C - CREATE Operations
        // ------------------------------------

        /// <summary>
        /// Creates a single sensor entity in the database.
        /// Before creation, it ensures the associated station exists by calling <see cref="EnsureStationExisting(Sensor?, string?)"/>.
        /// </summary>
        /// <param name="sensor">The <see cref="Sensor"/> entity to be stored.</param>
        /// <returns>A Task that returns the stored <see cref="Sensor"/> entity, or <c>null</c> if the operation fails.</returns>
        public async Task<Sensor?> Create(Sensor sensor)
        {
            _logger.StartTimer();

            await EnsureStationExisting(sensor);
            
            _context.Sensors.Add(sensor);
            try
            {
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Create", sensor);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }

            return sensor;
        }

        /// <summary>
        /// Creates a single sensor entity in the database, allowing an optional MAC address to be passed for station lookup/creation.
        /// It ensures the associated station exists (or creates a new one) by calling <see cref="EnsureStationExisting(Sensor?, string?)"/>.
        /// </summary>
        /// <param name="sensor">The <see cref="Sensor"/> entity to be stored.</param>
        /// <param name="macAdress">Optional MAC address to identify or create the related <see cref="Station"/>.</param>
        /// <returns>A Task that returns the stored <see cref="Sensor"/> entity, or <c>null</c> if the operation fails.</returns>
        public async Task<Sensor?> Create(Sensor sensor, string? macAdress = null)
        {
            await EnsureStationExisting(sensor, macAdress);

            _logger.StartTimer();
            try
            {
                await TryExecuteAsync(async() => await _context.Sensors.AddAsync(sensor), "AddAsync", "Create", sensor);
                await TryExecuteAsync(async() => await _context.SaveChangesAsync(), "SaveChangesAsync", "Create", sensor);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            { 
                _logger.StopTimer(); 
            }
            
            return sensor;
        }
        /// <inheritdoc/>
        public async Task<List<Sensor>> CreateByList(List<Sensor> sensors)
        {
            _logger.StartTimer();
            try
            {
                foreach (var sensor in sensors)
                {
                    await TryExecuteAsync(async() => await _context.Sensors.AddAsync(sensor), "AddAsync", "Create", sensors);
                }

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "CreateByList", sensors);

            }
            catch (Exception)
            {

                return new List<Sensor>();
            }
            finally 
            { 
                _logger.StopTimer(); 
            }
            
            return sensors;
        }

        // ------------------------------------
        // R - READ Operations
        // ------------------------------------

        /// <inheritdoc/>
        public async Task<Sensor?> GetById(int id)
        {
            _logger.StartTimer();
            Sensor? sensorFromDb;
            try
            {
                sensorFromDb = await TryExecuteAsync(async() => await _context.Sensors.FindAsync(id), "FindAsync", "GetById", id);

            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }

            return sensorFromDb;
        }
        /// <summary>
        /// Retrieves a list of sensors associated with a specific station ID.
        /// </summary>
        /// <param name="stationId">The unique identifier of the station.</param>
        /// <returns>A Task that returns a list of <see cref="Sensor"/> entities. Returns an empty list if the station has no sensors or on failure.</returns>
        public async Task<List<Sensor>> GetByStationId(int stationId)
        {
            _logger.StartTimer();

            List<Sensor>? sensorsFromDb;

            try
            {
                sensorsFromDb = await TryExecuteAsync(async () => await _context.Sensors.Where(s => s.StationId == stationId).ToListAsync(), "ToListAsync", "GetSensorsByStationId", stationId);
            }
            catch (Exception)
            {
                return new List<Sensor>();
            }
            finally
            {
                _logger.StopTimer();
            }

            if (sensorsFromDb == null)
            {
                return new List<Sensor>();
            }

            return sensorsFromDb;
        }
        /// <summary>
        /// Retrieves a single sensor entity by its station ID and sensor type.
        /// </summary>
        /// <param name="stationId">The unique identifier of the associated station.</param>
        /// <param name="type">The type of the sensor to search for.</param>
        /// <returns>A Task that returns the found <see cref="Sensor"/> entity, or <c>null</c> if no matching sensor is found or on failure.</returns>
        public async Task<Sensor?> GetSensorsByStationIdAndType(int stationId, string type)
        {
            _logger.StartTimer();
            Sensor? sensorFromDb;

            try
            {
                sensorFromDb = await TryExecuteAsync(async () => await _context.Sensors.FirstOrDefaultAsync(s => s.StationId == stationId && s.Type == type),
                "FirstOrDefaultAsync", "GetSensorsByStationIdAndType", stationId);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }
            
            return sensorFromDb;
        }
        /// <summary>
        /// Retrieves a list of sensors based on a list of their unique IDs.
        /// </summary>
        /// <param name="ids">The list of sensor IDs to retrieve.</param>
        /// <returns>A Task that returns a list of found <see cref="Sensor"/> entities. Returns an empty list if no matching sensors are found or on failure.</returns>
        public async Task<List<Sensor>> GetByListOfIds(List<int> ids)
        {
            _logger.StartTimer();

            List<Sensor> sensorsFromDb = new List<Sensor>();
            try
            {
                foreach (int id in ids)
                {
                    Sensor? sensor = await TryExecuteAsync(async () => await _context.Sensors.FindAsync(id), "FindAsync", "GetByListOfIds", id);
                    if (sensor != null)
                    {
                        sensorsFromDb.Add(sensor);
                    }
                }
            }
            catch (Exception)
            {
                return sensorsFromDb;
            }
            finally
            {
                _logger.StopTimer();
            }

            return sensorsFromDb;
        }
        /// <summary>
        /// Retrieves a list of all sensors associated with a specific station ID.
        /// </summary>
        /// <param name="stationId">The unique identifier of the station.</param>
        /// <returns>A Task that returns a list of <see cref="Sensor"/> entities. Returns an empty list if the ID is invalid, the station has no sensors, or on failure.</returns>
        public async Task<List<Sensor>> GetListByStationId(int stationId)
        {
            if(stationId <= 0)
            {
                return new List<Sensor>();
            }

            _logger.StartTimer();

            List<Sensor>? sensorsFromDb;

            try
            {
                sensorsFromDb = await TryExecuteAsync(async() => await _context.Sensors.Where(s => s.StationId == stationId).ToListAsync(), "ToListAsync", "GetListByStationId");
                if( sensorsFromDb == null)
                {  
                    return new List<Sensor>(); 
                }

                return sensorsFromDb;
        
            }
            catch (Exception)
            {
                return new List<Sensor>();
            }
            finally
            {
                _logger.StopTimer();
            }}
        /// <summary>
        /// Retrieves a list of IDs for all sensors associated with a specific station ID.
        /// </summary>
        /// <param name="stationId">The unique identifier of the station.</param>
        /// <returns>A Task that returns a list of sensor IDs (<see cref="int"/>). Returns an empty list if the ID is invalid, the station has no sensors, or on failure.</returns>
        public async Task<List<int>> GetIdsByStationId(int stationId)
        {
            if(stationId <= 0)
            {
                return new List<int>();
            }

            _logger.StartTimer();

            List<int>? sensorIdsFromDb;

            try
            {
                sensorIdsFromDb = await TryExecuteAsync(async() => await _context.Sensors.Where(s => s.StationId == stationId).Select(s => s.Id).ToListAsync(), "ToListAsync", "GetIdsByStationId");
                
                if( sensorIdsFromDb == null)
                {  
                    return new List<int>(); 
                }

                return sensorIdsFromDb;
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
        /// <inheritdoc/>
        public async Task<List<Sensor>> GetAll()
        {
            _logger.StartTimer();

            List<Sensor>? sensorsFromDb;
            try
            {
                sensorsFromDb = await TryExecuteAsync(async () => await _context.Sensors.ToListAsync(), "ToListAsync", "GetAll");

                if (sensorsFromDb == null)
                {
                    return new List<Sensor>();
                }

                return sensorsFromDb;
            }
            catch (Exception)
            {
                return new List<Sensor>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Retrieves a list of all sensor IDs present in the database.
        /// </summary>
        /// <returns>A Task that returns a list of all sensor IDs (<see cref="int"/>). Returns an empty list on failure.</returns>
        public async Task<List<int>> GetAllIds()
        {
            _logger.StartTimer();

            try
            {
                List<int>? sensorIdsFromContext;
            
                sensorIdsFromContext = await TryExecuteAsync(async () => await _context.Sensors.Select(s => s.Id).ToListAsync(), "ToListAsync", "GetAllIds");
            
                if (sensorIdsFromContext == null)
                {
                    return new List<int>();
                }
                return sensorIdsFromContext;
            }
            catch (Exception)
            {
                return new List<int>();
            }
        }


        // ------------------------------------
        // U - UPDATE Operations
        // ------------------------------------

        /// <inheritdoc/>
        public async Task<Sensor?> Update(Sensor sensor)
        {
            _logger.StartTimer();
            
            try
            {
                Sensor? sensorFromDb = await TryExecuteAsync(async () => await _context.Sensors.FindAsync(sensor.Id), "", "", sensor);

                if (sensorFromDb != null)
                {
                    sensorFromDb.Update(sensor);

                    await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Update", sensor);
                }

                return sensorFromDb;
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
        public async Task<List<Sensor>> UpdateByList(List<Sensor> sensors)
        {
            if(sensors.IsNullOrEmpty())
            {
                return new List<Sensor>();
            }

            _logger.StartTimer();

            List<Sensor> sensorsFromDb = new List<Sensor>();
            try
            {
                sensorsFromDb =  await TryExecuteAsync(async () => await _context.Sensors.ToListAsync(), "ToListAsync", "UpdateByList", sensors);

                if(sensorsFromDb.IsNullOrEmpty())
                {
                    return new List<Sensor>();
                }

                Dictionary<int,Sensor> sensorDict = sensors.ToDictionary(s => s.Id, s=>s);

                foreach (Sensor sensor in sensorsFromDb)
                {
                    if(sensorDict.TryGetValue(sensor.Id, out Sensor? existingSensor))
                    {
                        existingSensor.Update(sensor);
                    }            
                }

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "UpdateByList", sensors);

                return sensorsFromDb;
            }
            catch (Exception)
            {
                return new List<Sensor>();
            }
            finally 
            { 
                _logger.StopTimer(); 
            }
        }


        // ------------------------------------
        // D - DELETE Operations
        // ------------------------------------

        /// <summary>
        /// Deletes a sensor entity from the database based on its ID.
        /// </summary>
        /// <param name="sensorId">The unique identifier of the sensor.</param>
        /// <returns>A Task that returns the deleted <see cref="Sensor"/> entity, or <c>null</c> if the entity was not found, the ID is invalid, or deletion fails.</returns>
        public async Task<Sensor?> Delete(int sensorId)
        {
            if(sensorId <=  0)
            {  
                return null; 
            }
            
            _logger?.StartTimer();
            
            try
            {
                Sensor?sensorFromDb = await TryExecuteAsync(async () => await _context.Sensors.FindAsync(sensorId), "FindAsync", "Delete");

                if (sensorFromDb != null)
                {
                    await TryExecuteAsync(() =>
                    {
                        _context.Sensors.Remove(sensorFromDb);
                        return Task.FromResult<object>(null);
                    }, "Remove", "Delete", sensorFromDb);

                    await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Delete", sensorFromDb);
                }

                return sensorFromDb;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _logger?.StopTimer();
            }
        }
        /// <summary>
        /// Deletes all sensors associated with a specific station ID.
        /// </summary>
        /// <param name="stationId">The unique identifier of the station whose sensors should be deleted.</param>
        /// <returns>A Task that returns a list of the deleted <see cref="Sensor"/> entities. Returns an empty list if the station has no sensors or on failure.</returns>
        public async Task<List<Sensor>>? DeleteListByStationId(int stationId)
        {
            if(stationId <= 0)
            {
                return new List<Sensor>();
            }

            _logger.StartTimer(); 

            try
            {
                List<Sensor> sensorsFromDb = _context.Sensors.Where(s =>s.StationId==stationId).ToList();

                if (sensorsFromDb.Count > 0) 
                {
                    await TryExecuteAsync(() =>
                    {
                        _context.Sensors.RemoveRange(sensorsFromDb);
                        return Task.FromResult<object>(null);
                    }, "RemoveRange", "DeleteListByStationId", sensorsFromDb);
                
                    await TryExecuteAsync(async() =>  await _context.SaveChangesAsync(), "SaveChangesAsync", "DeleteListByStationId", sensorsFromDb);                
                }

                return sensorsFromDb;
            }
            catch (Exception)
            {
                return new List<Sensor>();
            }
            finally 
            { 
                _logger?.StopTimer(); 
            }
        }
        /// <summary>
        /// Deletes a list of sensor entities from the database.
        /// </summary>
        /// <param name="sensors">The list of <see cref="Sensor"/> entities to be deleted.</param>
        /// <returns>A Task that returns the list of deleted <see cref="Sensor"/> entities. Returns an empty list if the input is empty or on failure.</returns>
        public async Task<List<Sensor>>? DeleteList(List<Sensor> sensors)
        {
            if(sensors.IsNullOrEmpty())
            {
                return new List<Sensor>();
            }

            _logger.StartTimer();
            try
            {
                await TryExecuteAsync(
                    () =>
                    {
                        _context.Sensors.RemoveRange(sensors);
                        return Task.FromResult<object>(null);
                    }, "RemoveRange", "DeleteList", sensors);
                
                await TryExecuteAsync(async() => await _context.SaveChangesAsync(), "SaveChangesAsync", "DeleteList", sensors);

                return sensors;
            }
            catch (Exception)
            {
                return new List<Sensor>();
            }
            finally 
            { 
                _logger?.StopTimer(); 
            }
        }
        /// <inheritdoc/>
        public async Task<List<Sensor>> DeleteAll()
        {
            _logger.StartTimer();

            try
            {
                List<Sensor>? sensorsFromDb = await TryExecuteAsync(async() => await _context.Sensors.ToListAsync(), "ToListAsync", "DeleteAll");
                
                if(sensorsFromDb == null)
                {
                    return new List<Sensor>();
                }

                if(sensorsFromDb!.Any())
                {
                    await TryExecuteAsync(
                        () =>
                        {
                            _context.Sensors.RemoveRange(sensorsFromDb!);
                            return Task.FromResult<object>(null!);
                        }, "RemoveRange", "DeleteAll", sensorsFromDb);
                }

                return sensorsFromDb;
            }
            catch (Exception)
            {
                return new List<Sensor>();
            }
            finally 
            { 
                _logger?.StopTimer(); 
            }
        }
        /// <inheritdoc/>
        public async Task<List<Sensor>> DeleteByListOfIds(List<int> ids)
        {
            if(ids.IsNullOrEmpty())
            {
                return new List<Sensor>();
            }

            List<Sensor> sensorsFromDb = new List<Sensor>();

            try
            {
                foreach (int id in ids)
                {
                    Sensor? sensor = _context.Sensors.Find(id);

                    if (sensor != null)
                    {
                        sensorsFromDb.Add(sensor);
                    }
                }
                if (sensorsFromDb.Count > 0)
                {
                    await TryExecuteAsync(
                        () =>
                        {
                            _context.Sensors.RemoveRange(sensorsFromDb);
                            return Task.FromResult<object>(null!);
                        }, "RemoveRange", "DeleteByListOfIds", sensorsFromDb);
                }

                return sensorsFromDb;
            }
            catch (Exception)
            {
                return new List<Sensor>();
            }
            finally 
            { 
                _logger?.StopTimer(); 
            }
        }

        // ------------------------------------
        // Helper Methods
        // ------------------------------------

        /// <summary>
        /// Checks if the station linked to the sensor (via <c>StationId</c> or <c>macAddress</c>) exists.
        /// If the station does not exist, a new one is created with default values and the sensor's <c>StationId</c> is updated accordingly.
        /// </summary>
        /// <param name="sensor">The sensor entity containing the <c>StationId</c>.</param>
        /// <param name="macAddress">Optional MAC address used for station lookup/creation, prioritizing it over <c>sensor.StationId</c>.</param>
        /// <returns>A Task that returns the existing or newly created <see cref="Station"/> entity, or <c>null</c> if both <c>sensor</c> and <c>macAddress</c> are null/empty.</returns>
        public async Task<Station?> EnsureStationExisting(Sensor? sensor, string? macAddress = null)
        {
            Station? station;

            if (string.IsNullOrEmpty(macAddress) && sensor == null)
            {
                return null;
            }
            else if (sensor != null && string.IsNullOrEmpty(macAddress))
            {
                station = await _stationRepo.GetById(sensor.StationId);
            }
            else
            {
                station = await stationRepo.GetByMacAdress(macAddress!);
            }

            if (station == null)
            {
                Station newStation = new Station()
                {
                    MacAddress = macAddress,
                    SensorsCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                station = await _stationRepo.Create(newStation);
            }

            if (sensor != null)
            {
                sensor.StationId = station.Id;
            }

            return station;
        }
    }
}
