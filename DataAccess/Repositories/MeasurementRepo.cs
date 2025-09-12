using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;
using LoggingService;

namespace DataAccess.Repositories
{
    public class MeasurementRepo(ApiContext context, ILoggingService logger, ISensorRepo sensorRepo, IStationRepo stationRepo) : BaseRepo(context, logger), IRepo<Measurement>, IMeasurementRepo
    {
        private readonly ISensorRepo _sensorRepo = sensorRepo; // Hinzufügen des SensorRepo als Abhängigkeit
        private readonly IStationRepo _stationRepo = stationRepo;

        public async Task<Measurement?> Create(Measurement measurement)
        {
            if (measurement == null)
            {
                return null;
            }

            await EnsureSensorExisting(measurement);

            _logger.StartTimer();

            try
            {
                await TryExecuteAsync(async () => await _context.Measurements.AddAsync(measurement), "Add Measurement");
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "Save Changes");

                _logger.Log("Measurement", "created", measurement);

                return measurement;
            }
            catch (Exception ex)
            {
                _logger.Log("Error", "creation failed", ex.Message);

                return null;
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        public async Task<Measurement?> Create(Measurement measurement, string? macAddress = null)
        {
            if (measurement == null)
            {
                return null;
            }

            await EnsureSensorExisting(measurement, macAddress);

            _logger.StartTimer();

            try
            {
                await _context.Measurements.AddAsync(measurement);
                await SaveChanges();

                _logger.Log("Measurement", "created", measurement);
                return measurement;

            }
            catch (Exception ex)
            {
                _logger.Log("Error", "creation faild", ex.Message);
                return null;
            }
            finally
            {
                _logger.StopTimer();
            }
            
        }
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
                    await EnsureSensorExisting(measurement);

                    await _context.Measurements.AddAsync(measurement);
                }

                await SaveChanges();

                _logger.Log("Measurements", "created", measurements);

                return measurements;
            }
            catch (Exception ex)
            {
                _logger.Log("Error", "creation failed", ex.Message);
                return new List<Measurement>();
            }
            finally
            {  
                _logger.StopTimer(); 
            }
        }

        public async Task<List<Measurement>> GetAllAsList()
        {
            _logger.StartTimer();

            List<Measurement> measurementsFromDb;

            try
            {
                measurementsFromDb = await _context.Measurements.ToListAsync();

                _logger.Log("Measurements", "queried", measurementsFromDb);

                return measurementsFromDb;
            }
            catch (Exception ex)
            {
                _logger.Log("Error", "query failed", ex.Message);

                return new List<Measurement>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        public async Task<Measurement?> GetById(int id)
        {
            if(id <= 0)
            {
                return null;
            }

            _logger.StartTimer();
            
            Measurement? measurementFromDb = await _context.Measurements.FindAsync(id);

            if(measurementFromDb == null)
            {
                _logger.Log("Measurement", "queried", measurementFromDb);
                _logger.StopTimer();

                return null;
            }

            _logger.StopTimer();

            return measurementFromDb;
        }
        public async Task<List<Measurement>> GetAllBySensorIdAsList(int sensorId)
        {
            _logger.StartTimer();

            List <Measurement> measurements = await _context.Measurements.Where(s => s.SensorId == sensorId).ToListAsync();

            _logger.Log("Measurements", "queried", measurements);  

            return measurements;
        }
        public async Task<List<Measurement>> GetByListOfIds(List<int> ids)
        {
            List<Measurement> measurementsFromDb = new List<Measurement>();

            _logger.StartTimer();

            if (!ids.IsNullOrEmpty())
            {
                foreach (int id in ids)
                {
                    Measurement? measurement = await _context.Measurements.FindAsync(id);

                    if (measurement != null)
                    {
                        measurementsFromDb.Add(measurement);
                    }
                }
            }

            _logger.Log("Measurements", "queried", measurementsFromDb);
            _logger.StopTimer();

            return measurementsFromDb;
        }
        public async Task<List<Measurement>> GetAll()
        {
            _logger.StartTimer();

            List<Measurement> measurementsFromDb = await _context.Measurements.ToListAsync();

            _logger.Log("Measurements", "queried", measurementsFromDb);

            return measurementsFromDb;
        }

        public async Task<Measurement?> Update(Measurement measurement)
        {
            _logger.StartTimer();

            Measurement measurementFromDb = await _context.Measurements.FindAsync(measurement.Id);
            
            if (measurementFromDb != null)
            {
                measurementFromDb.Update(measurement);

                await SaveChanges();
                _logger.Log("Measurement", "updated", measurementFromDb);
                _logger.StopTimer();

                return measurementFromDb;
            }

            return null;
        }
        public async Task<List<Measurement>> UpdateByList(List<Measurement> measurements)
        {
            if(measurements.IsNullOrEmpty())
            {
                return new List<Measurement>();
            }

            List<Measurement> measurementsFromDb = new List<Measurement>();
            Measurement? measurementFromDb;

            _logger.StartTimer();

            foreach (Measurement measurement in measurements)
            {
                measurementFromDb = await _context.Measurements.FindAsync(measurement.Id);

                if (measurementFromDb != null)
                {
                    measurementFromDb.Update(measurement);

                    measurementsFromDb.Add(measurementFromDb);
                }
            }

            await SaveChanges();

            _logger.Log("Measurements", "updated", measurementsFromDb);
            _logger.StopTimer();

            return measurementsFromDb;
        }

        public async Task<Measurement?> DeleteById(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Measurement? measurementFromDb = await _context.Measurements.FindAsync(id);

            if (measurementFromDb != null)
            {
                _context.Measurements.Remove(measurementFromDb);

                await SaveChanges();

                LogOperationTime(stopwatch, "Measurement", "deleted", measurementFromDb);
            }

            return measurementFromDb;
        }
        public async Task<List<Measurement>> DeleteAll()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Measurement> measurementsFromDb = await _context.Measurements.ToListAsync();

            _context.Measurements.RemoveRange(measurementsFromDb);
            
            await SaveChanges();

            LogOperationTime(stopwatch, "Measurements", "deleted", measurementsFromDb);

            return measurementsFromDb;
        } 
        public async Task<List<Measurement>> DeleteMeasurmentsBySensorId(int sensorId)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Measurement> measurementsFromDb = await _context.Measurements.Where(m=>m.SensorId==sensorId).ToListAsync();

            _context.Measurements.RemoveRange(measurementsFromDb);
            
            await SaveChanges();

            LogOperationTime(stopwatch, "Measurements", "deleted", measurementsFromDb);

            return measurementsFromDb;
        }
        public async Task<List<Measurement>> DeleteByListOfIds(List<int> ids)
        {
            Stopwatch stopwatch= Stopwatch.StartNew();

            List<Measurement> measurementsFromDb = await GetByListOfIds(ids);

            if (!measurementsFromDb.IsNullOrEmpty())
            {
                _context.Measurements.RemoveRange(measurementsFromDb);
            }

            await SaveChanges();

            LogOperationTime(stopwatch, "Measurements", "deleted", measurementsFromDb);

            return measurementsFromDb;
        }
        //TODO: DeleteMeasurmentsByStation needs a ReferenceTable for Deleting By StationId

        //public List<Measurement> DeleteMeasurmentsByStationId(int stationId)
        //{
        //    List<Measurement> measurements = new List<Measurement>();
        //    try
        //    {
        //        measurements = _context.Measurements.Where(m=>m.stationId);
        //        _context.Measurements.RemoveRange(measurements);
        //        _context.SaveChanges();
        //    }
        //    catch (Exception)
        //    {
        //        //TODO: Error logging
        //    }

        //    return measurements;
        //}

        private async Task<bool> EnsureSensorExisting(Measurement measurement)
        {
            bool sensorExisted = false;
            Sensor? sensor = null;

            if (measurement != null)
            { 
                sensor = await _sensorRepo.GetById(measurement.SensorId);

                sensorExisted = true;

                if (sensor == null)
                {
                    Sensor newSensor = new Sensor() { DeviceId = 0, StationId = 0 };
                    Sensor createdSensor = await _sensorRepo.Create(newSensor);
                    //sensor = await _context.Sensors.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
                    //measurement.SensorId = createdSensor.Id;

                    sensorExisted = false;
                }
            }

            return sensorExisted;
        }
        
        private async Task<bool> EnsureSensorExisting(Measurement measurement, string? macAddress)
        {
            bool sensorExisted = false;
            Sensor? sensor = null;
            Station? station = null;

            if (measurement != null && _sensorRepo is SensorRepo sensorRepo && _stationRepo is StationRepo stationRepo)
            { 
                sensor = await sensorRepo.GetById(measurement.SensorId);

                station = await stationRepo.GetByMacAdress(macAddress);

                sensorExisted = true;

                if (station != null)
                {
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

                        Sensor createdSensor = await sensorRepo.Create(newSensor, macAddress);
                        //sensor = await _context.Sensors.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
                        //measurement.SensorId = createdSensor.Id;

                        measurement.SensorId = newSensor.Id;
                        sensorExisted = false;
                    }

                    await stationRepo.Update(station);
                }
            }

            return sensorExisted;
        }

    }
}
