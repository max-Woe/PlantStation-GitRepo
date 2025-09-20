using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;
using Serilog;
using LoggingService;

namespace DataAccess.Repositories
{
    public class SensorRepo(ApiContext context, ILoggingService logger, IStationRepo stationRepo) : BaseRepo(context, logger), IRepo<Sensor>, ISensorRepo
    {
        private readonly IStationRepo _stationRepo = stationRepo;
        public async Task<Sensor> Create(Sensor sensor)
        {
            _logger.StartTimer();
            //await EnsureStationExisting(sensor);
            
            _context.Sensors.Add(sensor);

            await SaveChanges();

            _logger.Log("Sensor", "created", sensor);

            return sensor;
        }

        public async Task<Sensor> Create(Sensor sensor, string? macAdress = null)
        {
            await EnsureStationExisting(sensor, macAdress);

            _logger.StartTimer();

            if (_stationRepo is StationRepo stationRepo)
            {

            }

            _context.Sensors.Add(sensor);

            await SaveChanges();

            _logger.Log("Sensor", "created", sensor);

            return sensor;
        }

        public async Task<List<Sensor>> CreateByList(List<Sensor> sensors)
        {
            _logger.StartTimer();

            foreach (var entity in sensors)
            {
                _context.Sensors.Add(entity);
            }

            await SaveChanges();

            _logger.Log("Sensors", "created", sensors);

            return sensors;
        }


        public async Task<Sensor?> GetById(int id)
        {
            _logger.StartTimer();
            
            Sensor? sensorFromDb = await _context.Sensors.FindAsync(id);

            if (sensorFromDb != null)
            {
                _logger.Log("Sensor", "queried", sensorFromDb);
            }

            return sensorFromDb;
        }
        public async Task<List<Sensor>> GetSensorsByStationId(int stationId)
        {
            _logger.StartTimer();

            List<Sensor> sensorsFromDb = _context.Sensors.Where(s => s.StationId == stationId).ToList();

            _logger.Log("Sensors", "queried", sensorsFromDb);

            return sensorsFromDb;
        }


        public async Task<Sensor?> GetSensorsByStationIdAndType(int stationId, string type)
        {
            _logger.StartTimer();

            Sensor? sensorFromDb = await _context.Sensors.FirstOrDefaultAsync(s => s.StationId == stationId && s.Type == type);

            _logger.Log("Sensor", "queried", sensorFromDb);

            return sensorFromDb;
        }



        public async Task<List<Sensor>> GetByListOfIds(List<int> ids)
        {
            _logger.StartTimer();

            List<Sensor> sensorsFromDb = new List<Sensor>();

            foreach (int id in ids)
            {
                sensorsFromDb.Add(_context.Sensors.Find(id));
            }

            //LogOperationTime(stopwatch, "Sensors", "queried", sensorsFromDb);
            _logger.Log("Sensors", "queried", sensorsFromDb);

            return sensorsFromDb;
        }
        public async Task<List<Sensor>> GetAll()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Sensor> sensorsFromDb = _context.Sensors.ToList();

            LogOperationTime(stopwatch, "Sensors", "queried", sensorsFromDb);

            return sensorsFromDb;
        }


        public async Task<Sensor?> Update(Sensor sensor)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Sensor sensorFromDb = _context.Sensors.Find(sensor.Id);

            if (sensorFromDb != null)
            {
                sensorFromDb.Update(sensor);
                    
                await SaveChanges();
            }

            LogOperationTime(stopwatch, "Sensor", "updated", sensorFromDb);

            return sensorFromDb;
        }
        public async Task<List<Sensor>> UpdateByList(List<Sensor> sensors)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Sensor> sensorsFromDb = new List<Sensor>();

            if (sensorsFromDb.Count > 0)
            {
                foreach (Sensor sensor in sensors)
                {
                    Sensor sensorFromDb = _context.Sensors.Find(sensor.Id);
                    sensorsFromDb.Add(sensorFromDb);
                    sensorFromDb.Update(sensor);
                }

                 await SaveChanges();

                LogOperationTime(stopwatch, "Sensors", "updated", sensorsFromDb);

            }
            else
            {
                stopwatch.Stop();
            }

            return sensorsFromDb;
        }
        
        public async Task<Sensor?> DeleteById(int sensorId)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Sensor sensorFromDb = _context.Sensors.Find(sensorId);

            if (sensorFromDb != null) 
            {
                _context.Sensors.Remove(sensorFromDb);
                
                await SaveChanges();
            }

            LogOperationTime(stopwatch, "Sensor", "deleted", sensorFromDb);

            return sensorFromDb;
        }
        public async Task<List<Sensor>>? DeleteSensorByStationId(int stationId)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Sensor> sensorsFromDb = _context.Sensors.Where(s =>s.StationId==stationId).ToList();

            if (sensorsFromDb.Count > 0) 
            {
                _context.Sensors.RemoveRange(sensorsFromDb);
                
                await SaveChanges();
            }

            LogOperationTime(stopwatch, "Sensors", "deleted", sensorsFromDb);

            return sensorsFromDb;
        }
        public async Task<List<Sensor>>? DeleteListOfSensors(List<Sensor> sensors)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            _context.Sensors.RemoveRange(sensors);

            await SaveChanges();

            LogOperationTime(stopwatch, "Sensors", "deleted", sensors);

            return sensors;
        }
        public async Task<List<Sensor>> DeleteAllSensors()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Sensor> sensorsFromDb = new List<Sensor>();

            if (sensorsFromDb.Count > 0)
            {
                sensorsFromDb = _context.Sensors.ToList();

                await SaveChanges();
            }

            LogOperationTime(stopwatch, "Sensors", "deleted", sensorsFromDb);

            return sensorsFromDb;
        }
        public async Task<List<Sensor>> DeleteByListOfIds(List<int> ids)
        {
            List<Sensor> sensorsFromDb = new List<Sensor>();
            Sensor? sensor;

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (int id in ids)
            {
                sensor = _context.Sensors.Find(id);

                if (sensor != null)
                {
                    sensorsFromDb.Add(sensor);

                    _context.Sensors.Remove(sensor);
                }
            }

            if (sensorsFromDb.Count > 0)
            {
                await SaveChanges();

                LogOperationTime(stopwatch, "Sensors", "deleted", sensorsFromDb);
            }

            return sensorsFromDb;
        }
        public async Task<List<Sensor>> DeleteAll()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Sensor> sensorsFromDb = _context.Sensors.ToList();

            _context.Sensors.RemoveRange(sensorsFromDb);

            await SaveChanges();

            LogOperationTime(stopwatch, "Sensors", "deleted", sensorsFromDb);

            return sensorsFromDb;
        }

        private async Task<bool> EnsureStationExisting(Sensor sensor, string? macAddress)
        {
            bool stationExisted = false;
            Station station;

            if (macAddress != null && _stationRepo is StationRepo stationRepo)
            {
                station = await stationRepo.GetByMacAdress(macAddress);

                stationExisted = true;

                if (station == null)
                {
                    Station newStation = new Station()
                    {
                        MacAddress = macAddress,
                        SensorsCount = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    station = newStation;

                    Station createdStation = await _stationRepo.Create(newStation);

                    stationExisted = false;
                }
                sensor.StationId = station.Id;
            }
         
            return stationExisted;
        }

        public async Task<List<Sensor>> GetListByStationId(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Sensor> sensorsFromDb = await _context.Sensors.Where(s => s.StationId == id).ToListAsync();
            LogOperationTime(stopwatch, "Sensors", "queried", sensorsFromDb);
            return sensorsFromDb;
        }

        public async Task<bool> CheckForSensorByStationIdAndUnit(int stationId, string unit)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool exists = await _context.Sensors.AnyAsync(s => s.StationId == stationId && s.Unit == unit);
            LogOperationTime(stopwatch, "Sensor", "checked existence", new { StationId = stationId, Unit = unit, Exists = exists });
            return exists;
        }

        public async Task<bool> CheckForSensorByStationIdAndType(int stationId, string type)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool exists = await _context.Sensors.AnyAsync(s => s.StationId == stationId && s.Unit == type);
            LogOperationTime(stopwatch, "Sensor", "checked existence", new { StationId = stationId, Unit = type, Exists = exists });
            return exists;
        }
    }
}
