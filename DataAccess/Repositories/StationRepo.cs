using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using LoggingService;

namespace DataAccess.Repositories 
{
    public class StationRepo(ApiContext context, ILoggingService logger) : BaseRepo(context, logger), IRepo<Station>, IStationRepo
    {
        public async Task<Station> Create(Station station)
        {
            _logger.StartTimer();

            _context.Stations.Add(station);
            await SaveChanges();

            _logger.Log("Station", "created", station);
            
            return station;
        }

        public async Task<Station?> GetByMacAdress(string macAdress)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Station? stationFromDb = _context.Stations.FirstOrDefault(s => s.MacAddress == macAdress);
            LogOperationTime(stopwatch, "Station", "queried", stationFromDb);

            return stationFromDb;
        }
        public async Task<List<Station>> CreateByList(List<Station> stations)
        {
            if(!stations.IsNullOrEmpty())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                foreach (var station in stations)
                {
                    _context.Stations.Add(station);
                }

                SaveChanges();

                LogOperationTime(stopwatch, "Stations", "created", stations);
            }        

            return stations;
        }

        public async Task<Station>? GetById(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Station stationFromDb = _context.Stations.Find(id);

            LogOperationTime(stopwatch, "Station", "queried", stationFromDb);

            return stationFromDb;
        }
        public async Task<List<Station>> GetByListOfIds(List<int> ids)
        {
            List<Station> stationsFromDb = new List<Station>();

            Station? stationFromDb;

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (int id in ids)
            {
                stationFromDb = _context.Stations.Find(id);

                stationsFromDb.Add(stationFromDb);
            }

            if(stationsFromDb.Count > 0)
            {
                SaveChanges();

                LogOperationTime(stopwatch, "Stations", "queried", stationsFromDb);
            }

            return stationsFromDb;
        }
        public async Task<List<Station>> GetAll()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();  
            
            List<Station> stationsFromDb = _context.Stations.ToList();

            LogOperationTime(stopwatch, "Stations", "queried", stationsFromDb);

            return stationsFromDb;
        }

        public async Task<Station?> Update(Station station)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Station? stationFromDb = await _context.Stations.FindAsync(station.Id);

            if (stationFromDb != null)
            {
                stationFromDb.Update(station);
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    Log.Error(ex, ex.Message);
                }
            }

            LogOperationTime(stopwatch, "Station", "updated", stationFromDb);

            return stationFromDb;
        }
        public async Task<List<Station>> UpdateStations(List<Station> stations)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Station> stationsFromDb = new List<Station>();
            Station stationFromDb = new Station();

            foreach (Station station in stations)
            {
                stationFromDb = _context.Stations.Find(station.Id);

                if (stationFromDb != null)
                {
                    stationFromDb.Update(station);

                    stationsFromDb.Add(stationFromDb);
                }
            }
            
            SaveChanges();

            LogOperationTime(stopwatch, "Stations", "queried", stationsFromDb);

            return stationsFromDb;
        }
        public async Task<List<Station>> UpdateByList(List<Station> stations)
        {
            List<Station> stationsFromDb = new List<Station>();

            Station? stationFromDb;

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (Station station in stations)
            {
                stationFromDb = _context.Stations.Find(station.Id);

                if (stationFromDb != null)
                {
                    stationFromDb.Update(station);
                    stationsFromDb.Add(stationFromDb);            
                }
            }

            if (stationsFromDb.Count > 0)
            {
                SaveChanges();

                LogOperationTime(stopwatch, "Stations", "updated", stationsFromDb);
            }

            return stationsFromDb;
        }

        public async Task<Station?> DeleteById(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Station? stationFromDb = await _context.Stations.FindAsync(id);

            if (stationFromDb != null)
            {
                _context.Remove(stationFromDb);
                _context.SaveChanges();
            }

            LogOperationTime(stopwatch, "Station", "queried", stationFromDb);

            return stationFromDb;
        }
        public async Task<List<Station>>? DeleteAllStations()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Station> stationsFromDb = _context.Stations.ToList();

            if (stationsFromDb.Count > 0)
            {
                _context.RemoveRange(stationsFromDb);

                SaveChanges();

                LogOperationTime(stopwatch, "Station", "queried", stationsFromDb);
            }

            return stationsFromDb;
        }

        public async Task<List<Station>> DeleteByListOfIds(List<int> ids)
        {
            List<Station> stationsFromDb = new List<Station>();

            Station? stationFromDb;

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (int id in ids)
            {
                stationFromDb = _context.Stations.Find(id);
                
                if(stationFromDb != null)
                { 
                    stationsFromDb.Add(stationFromDb); 
                }
            }

            if (stationsFromDb.Count > 0)
            {
                _context.RemoveRange(stationsFromDb);

                SaveChanges();

                LogOperationTime(stopwatch, "Stations", "deleted", stationsFromDb);
            }

            return stationsFromDb;
        }

        public async Task<List<Station>> DeleteAll()
        {
            Stopwatch stopwatch = Stopwatch.StartNew(); 

            List<Station> stationsFromDb = _context.Stations.ToList();

            if (stationsFromDb.Count > 0)
            {
                _context.RemoveRange(stationsFromDb);

                SaveChanges();

                LogOperationTime(stopwatch, "Stations", "deleted", stationsFromDb);
            }

            return stationsFromDb;
        }

    }
}
