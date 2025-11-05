using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;
using LoggingService;

namespace DataAccess.Repositories
{
    /// <summary>
    /// Implementation of the <see cref="IMeasurementRepo"/> interface.
    /// Provides database access (CRUD) and specific queries for <see cref="Measurement"/> objects.
    /// </summary>
    public class MeasurementRepo(IApiContext context, ILoggingService logger, ISensorRepo sensorRepo, IStationRepo stationRepo) : BaseRepo(context, logger), IRepo<Measurement>, IMeasurementRepo
    {
        private readonly ISensorRepo _sensorRepo = sensorRepo; // Hinzufügen des SensorRepo als Abhängigkeit
        private readonly IStationRepo _stationRepo = stationRepo;

        /// <summary>
        /// Creates a single measurement in the database without a MAC address and performs a sensor existence check.
        /// This is an overload of the interface method that only handles the <see cref="Measurement"/> parameter.
        /// </summary>
        /// <param name="measurement">The <see cref="Measurement"/> to store.</param>
        /// <returns>The stored measurement or <c>null</c> on error.</returns>
        public async Task<Measurement?> Create(Measurement measurement)
        {
            if (measurement == null)
            {
                _logger.LogError(new NullReferenceException(), "Nullcheck", "Create", null);
                return null;
            }

            _logger.StartTimer();

            Sensor? sensor = await EnsureSensorExisting(measurement, null);

            try
            {
                await TryExecuteAsync(async () => await _context.Measurements.AddAsync(measurement),"AddAsync", "Create", measurement);
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Create", measurement);

                return measurement;
            }
            catch
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<Measurement?> Create(Measurement measurement, string? macAddress = null)
        {
            if (measurement == null)
            {
                return null;
            }

            _logger.StartTimer();

            await EnsureSensorExisting(measurement, macAddress);

            try
            {
                await TryExecuteAsync(async () => await _context.Measurements.AddAsync(measurement), "AddAsync", "Create", measurement);
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Create", measurement);

                return measurement;

            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }

        }
        /// <summary>
        /// Creates a list of measurements in the database.
        /// Ensures that the corresponding sensors exist for each measurement.
        /// </summary>
        /// <param name="measurements">The list of <see cref="Measurement"/> objects to store.</param>
        /// <returns>The list of stored <see cref="Measurement"/> objects. Returns an empty list if the input list is empty or on error.</returns>
        public async Task<List<Measurement>> CreateByList(List<Measurement> measurements)
        {
            if(measurements.IsNullOrEmpty())
            {
                return new List<Measurement>();
            }

            _logger.StartTimer();

            try
            {
                foreach (Measurement measurement in measurements)
                {
                    await EnsureSensorExisting(measurement, null);

                }

                await TryExecuteAsync<int>(async () =>
                {
                    await _context.Measurements.AddRangeAsync(measurements);
                    return measurements.Count;
                }, "AddRangeAsync", "Create", measurements);

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Create", measurements);

                return measurements;
            }
            catch (Exception)
            {
                return new List<Measurement>();
            }
            finally
            {  
                _logger.StopTimer(); 
            }
        }
        
        /// <inheritdoc/>
        public async Task<List<Measurement>> GetAllAsList()
        {
            _logger.StartTimer();

            List<Measurement>? measurementsFromDb;

            try
            {
                measurementsFromDb = await TryExecuteAsync(async () => await _context.Measurements.ToListAsync(), "ToListAsync", "GetAllAsList");

                if(measurementsFromDb == null)
                {
                    _logger.StopTimer();

                    return new List<Measurement>();
                }

                return measurementsFromDb;
            }
            catch (Exception ex)
            {
                return new List<Measurement>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Retrieves a single measurement by its ID.
        /// This method is inherited from the generic <see cref="IRepo{T}"/>.
        /// </summary>
        /// <param name="id">The ID of the measurement to retrieve.</param>
        /// <returns>A Task that returns the found measurement or <c>null</c> if it does not exist or the ID is invalid.</returns>
        public async Task<Measurement?> GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            _logger.StartTimer();


            try
            {
                Measurement? measurementFromDb = await TryExecuteAsync(async () => await _context.Measurements.FindAsync(id), "FindAsync", "GetById");

                if (measurementFromDb == null)
                {
                    _logger.LogInformationText("Measurement not found in database.");
                    _logger.StopTimer();

                    return null;
                }

                return measurementFromDb;
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
        public async Task<List<Measurement>?> GetAllBySensorIdAsList(int sensorId)
        {
            _logger.StartTimer();

            List<Measurement>? measurements = new List<Measurement>();

            try
            {
                measurements = await TryExecuteAsync(async () => await _context.Measurements.Where(s => s.SensorId == sensorId).ToListAsync(),
                "ToListAsync", "GetAllBySensorIdAsList", sensorId);

                return measurements;
            }
            catch (Exception)
            {
                _logger.StopTimer();
                return measurements;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Retrieves a list of measurements based on a list of IDs.
        /// This method is inherited from the generic <see cref="IRepo{T}"/>.
        /// </summary>
        /// <param name="ids">The list of IDs for the measurements to retrieve.</param>
        /// <returns>A <see cref="List{T}"/> of the found <see cref="Measurement"/> objects. Invalid or non-existent IDs are ignored.</returns>
        public async Task<List<Measurement>> GetByListOfIds(List<int> ids)
        {
            List<Measurement> measurementsFromDb = new List<Measurement>();

            if (ids.IsNullOrEmpty())
            {
                return measurementsFromDb;
            }

            _logger.StartTimer();
            
            try
            {
                measurementsFromDb = await TryExecuteAsync(async () => await _context.Measurements.Where(m => ids.Contains(m.Id) ).ToListAsync(), "Find.Where", "GetByListOfIds");

                if(measurementsFromDb.IsNullOrEmpty())
                {
                    return new List<Measurement>();
                }

                return measurementsFromDb;
            }
            catch (Exception)
            {
                return measurementsFromDb;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Retrieves all existing measurements. This method is inherited from the generic <see cref="IRepo{T}"/>.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of all <see cref="Measurement"/> objects.</returns>
        public async Task<List<Measurement>> GetAll()
        {
            _logger.StartTimer();
            List<Measurement>? measurementsFromDb = new List<Measurement>();

            try
            {
                measurementsFromDb = await TryExecuteAsync(async () => await _context.Measurements.ToListAsync(), "ToListAsync", "GetAll");

                return measurementsFromDb!;
            }
            catch (Exception)
            {
                return measurementsFromDb!;
            }
            finally 
            { 
                _logger.StopTimer(); 
            }
        }


        /// <inheritdoc/>
        public async Task<List<Measurement>> GetLastOfSensor(int sensorId, int count)
        {
            if (count <= 0)
            {
                return new List<Measurement>();
            }
            _logger.StartTimer();

            List<Measurement>? measurementsFromDb = new List<Measurement>();

            try
            {
                measurementsFromDb = await TryExecuteAsync(async () => await _context.Measurements.Where(m => m.Id == sensorId).OrderByDescending(m => m.RecordedAt).Take(count).ToListAsync(), "ToListAsync", "GetLast");
                
                if(measurementsFromDb.IsNullOrEmpty())
                {
                    _logger.StopTimer();
                    return new List<Measurement>();
                }

                return measurementsFromDb;
            }
            catch (Exception)
            {
                return measurementsFromDb;
            }
            finally
            {
                _logger.StopTimer();
            }
        }

        /// <inheritdoc/>
        public async Task<List<Measurement>> GetLastOfSensorSince(int sensorId, DateTime since)
        {
            _logger.StartTimer();

            List<Measurement>? measurementsFromDb = new List<Measurement>();

            try
            {
                measurementsFromDb = await TryExecuteAsync(async () => await _context.Measurements.Where(m=> m.RecordedAt>since && m.SensorId == sensorId).OrderByDescending(m => m.RecordedAt).ToListAsync(), "ToListAsync", "GetLast");
                
                if(measurementsFromDb == null || measurementsFromDb.Count == 0)
                {
                    _logger.StopTimer();
                    return new List<Measurement>();
                }

                return measurementsFromDb;
            }
            catch (Exception)
            {
                return measurementsFromDb;
            }
            finally
            {
                _logger.StopTimer();
            }
        }

        /// <summary>
        /// Updates an existing measurement in the database.
        /// This method is inherited from the generic <see cref="IRepo{T}"/>.
        /// </summary>
        /// <param name="measurement">The <see cref="Measurement"/> object with the updated values. The ID must be set.</param>
        /// <returns>The updated measurement from the database or <c>null</c> if the measurement was not found.</returns>
        public async Task<Measurement?> Update(Measurement measurement)
        {
            _logger.StartTimer();
            Measurement? measurementFromDb;

            try
            {
                measurementFromDb = await TryExecuteAsync(async() => await _context.Measurements.FindAsync(measurement.Id), "FindAsync", "Update", measurement);

                if (measurementFromDb == null)
                {
                    _logger.LogInformationText($"Measurement with the id {measurement.Id} not found in database.");
                    return null;
                }

                await TryExecuteAsync<object>(() =>
                {
                    measurementFromDb.Update(measurement);
                    return Task.FromResult<object>(null);
                }, "Update", "Update", measurement);
                
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChanges", "Update", measurement);

                return measurementFromDb;
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
        /// Updates a list of existing measurements in the database.
        /// This method is inherited from the generic <see cref="IRepo{T}"/>.
        /// </summary>
        /// <param name="measurements">A <see cref="List{T}"/> of <see cref="Measurement"/> objects with the updated values. The IDs must be set.</param>
        /// <returns>A <see cref="List{T}"/> of the successfully updated <see cref="Measurement"/> objects.</returns>
        public async Task<List<Measurement>> UpdateByList(List<Measurement> measurements)
        {
            if(measurements.IsNullOrEmpty())
            {
                return new List<Measurement>();
            }

            List<Measurement>? measurementsFromDb = new List<Measurement>();
            Measurement? measurementFromDb;

            _logger.StartTimer();

            try
            {
                measurementsFromDb = await TryExecuteAsync(
                    async () => await GetByListOfIds(measurements.Select(m => m.Id).ToList()), 
                    "GetByListOfIds", 
                    "UpdateByList", 
                    measurements);
                
                if (measurementsFromDb.IsNullOrEmpty())
                {
                    return new List<Measurement>();
                }

                Dictionary<int,Measurement>? measurementsDict = measurements.ToDictionary(m => m.Id, m=>m);

                foreach (var measurement in measurementsFromDb)
                {
                    if(measurementsDict.TryGetValue(measurement.Id, out var existingMeasurement))
                    {
                        measurement.Update(existingMeasurement);
                    }
                }

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "UpdateByList", measurements);

                return measurementsFromDb;
            }
            catch (Exception)
            {

                return new List<Measurement>();
            }
            finally 
            { 
                _logger.StopTimer();
            }
        }

        /// <summary>
        /// Deletes a measurement by its ID.
        /// This method is inherited from the generic <see cref="IRepo{T}"/>.
        /// </summary>
        /// <param name="id">The ID of the measurement to delete.</param>
        /// <returns>The deleted measurement or <c>null</c> if it was not found or the ID is invalid.</returns>
        public async Task<Measurement?> Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogError(new ArgumentOutOfRangeException(), "ArgumentCheck", "DeleteById", id);
                return null;
            }
            _logger.StartTimer();

            Measurement? measurementFromDb;

            try
            {
                measurementFromDb = await TryExecuteAsync(async () => await _context.Measurements.FindAsync(id), "FindAsync", "DeleteById", id);
                if (measurementFromDb == null)
                {
                    _logger.LogInformationText($"Measurement with the id {id} not found in database.");
                    return null;
                }

                await TryExecuteAsync<object>(() =>
                {
                    _context.Measurements.Remove(measurementFromDb);
                    return Task.FromResult<object>(null);
                }, "Remove", "DeleteById", id);

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChanges", "DeleteById", id);

                return measurementFromDb;
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
        /// Deletes all measurements from the database.
        /// This method is inherited from the generic <see cref="IRepo{T}"/>.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of all deleted <see cref="Measurement"/> objects.</returns>
        public async Task<List<Measurement>> DeleteAll()
        {
            _logger.StartTimer();
            
            List<Measurement>? measurementsFromDb = new List<Measurement>();

            try
            {
                measurementsFromDb = await TryExecuteAsync(
                    async () => await _context.Measurements.ToListAsync(), 
                    "ToListAsync", 
                    "DeleteAll");

                if (measurementsFromDb.IsNullOrEmpty())
                {
                    return new List<Measurement>();
                }

                await TryExecuteAsync<object>(() =>
                {
                    _context.Measurements.RemoveRange(measurementsFromDb);
                    return Task.FromResult<object>(null);
                }, "RemoveRange", "DeleteAll", measurementsFromDb);

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChanges", "DeleteAll", measurementsFromDb);

                return measurementsFromDb!;
            }
            catch (Exception)
            {
                return measurementsFromDb!;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<List<Measurement>> DeleteMeasurmentsBySensorId(int sensorId)
        {
            _logger.StartTimer();
            List<Measurement>? measurementsFromDb = new List<Measurement>();
            try
            {

                measurementsFromDb = await TryExecuteAsync(
                    async() => await _context.Measurements.Where(m => m.SensorId == sensorId).ToListAsync(), 
                    "ToListAsync", 
                    "DeleteMeasurmentsBySensorId", 
                    null);

                if (measurementsFromDb.IsNullOrEmpty())
                {
                    return measurementsFromDb!;
                }

                await TryExecuteAsync<object>(() =>
                {
                    _context.Measurements.RemoveRange(measurementsFromDb);
                    return Task.FromResult<object>(null);
                }, "RemoveRange", "DeleteMeasurmentsBySensorId", sensorId);

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChanges", "DeleteMeasurmentsBySensorId", sensorId);
                
                return measurementsFromDb!;
            }
            catch (Exception)
            {
                return measurementsFromDb!;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <summary>
        /// Deletes a list of measurements based on a list of IDs.
        /// This method is inherited from the generic <see cref="IRepo{T}"/>.
        /// </summary>
        /// <param name="ids">The list of IDs for the measurements to delete.</param>
        /// <returns>A <see cref="List{T}"/> of the actually deleted <see cref="Measurement"/> objects.</returns>
        public async Task<List<Measurement>> DeleteByListOfIds(List<int> ids)
        {
            _logger.StartTimer();

            List<Measurement> measurementsFromDb = new List<Measurement>();
            try
            {
                measurementsFromDb = await TryExecuteAsync(async() => await GetByListOfIds(ids), "GetByListOfIds", "DeleteByListOfIds", null);

                if (measurementsFromDb.IsNullOrEmpty())
                {
                    return new List<Measurement>();
                }

                await TryExecuteAsync<object>(() =>
                {
                    _context.Measurements.RemoveRange(measurementsFromDb);
                    return Task.FromResult<object>(null);
                }, "RemoveRange", "DeleteByListOfIds", ids);
                
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChanges", "DeleteByListOfIds", ids);
                
                return measurementsFromDb!;

            }
            catch (Exception)
            {
                return measurementsFromDb!;
            }
        }
        //TODO: DeleteMeasurmentsByStation needs a ReferenceTable for Deleting By StationId

        /// <inheritdoc/>
        /// <remarks>
        /// Internal Logic: First calls <see cref="ISensorRepo.EnsureStationExisting(Sensor?, string?)"/> via the injected <see cref="ISensorRepo"/>.
        /// If the sensor in <paramref name="measurement"/> does not exist, a new sensor is created, its <see cref="Sensor.Id"/> is assigned to the measurement, and the station's <see cref="Station.SensorsCount"/> is updated.
        /// </remarks>
        public async Task<Sensor?> EnsureSensorExisting(Measurement measurement, string? macAddress)
        {
            if ( measurement == null || string.IsNullOrEmpty(macAddress))
            {
                return null;
            }

            Sensor? sensor = null;

            Station? station = await _sensorRepo.EnsureStationExisting(sensor, macAddress);
            if(station == null)
            {
                return null;
            }   

            sensor = await sensorRepo.GetById(measurement.SensorId);
            if (sensor == null)
            {
                station.SensorsCount++;
                Sensor newSensor = new Sensor()
                {
                    DeviceId = 0,
                    StationId = station.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                sensor = await sensorRepo.Create(newSensor);

                measurement.SensorId = newSensor.Id;
            }

            await stationRepo.Update(station);
            
            return sensor;
        }

    }
}
