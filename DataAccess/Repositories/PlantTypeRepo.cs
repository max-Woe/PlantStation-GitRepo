using System.Diagnostics;
using DataAccess.Models;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using LoggingService;

namespace DataAccess.Repositories
{
    public class PlantTypeRepo(ApiContext context, ILoggingService logger) : BaseRepo(context, logger), IRepo<PlantType>
    {
        public async Task<PlantType> Create(PlantType plantType)
        {
            _context.PlantTypes.Add(plantType);

            Stopwatch stopwatch = Stopwatch.StartNew();
            
            SaveChanges();

            LogOperationTime(stopwatch, "PlantType", "created", plantType);

            return plantType;
        }
        public async Task<List<PlantType>> CreateByList(List<PlantType> plantTypes)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (plantTypes.Count > 0)
            {

                foreach (var plantType in plantTypes)
                {
                    _context.PlantTypes.Add(plantType);
                }

                SaveChanges();

                LogOperationTime(stopwatch, "PlantTypes", "created", plantTypes);
            }

            return plantTypes;
        }

        public async Task<PlantType> GetById(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            PlantType plantTypeFromDb = _context.PlantTypes.Find(id);

            LogOperationTime(stopwatch, "PlantType", "queried", plantTypeFromDb);

            return plantTypeFromDb;
        }
        public async Task<List<PlantType>> GetAll()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<PlantType> plantTypesFromDb = _context.PlantTypes.ToList();

            LogOperationTime(stopwatch, "PlantTypes", "queried", plantTypesFromDb);

            return plantTypesFromDb;
        }
        public async Task<List<PlantType>> GetByListOfIds(List<int> ids)
        {
            List<PlantType> plantTypesFromDb = new List<PlantType>();

            PlantType? plantType;

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (int id in ids)
            {
                plantType = _context.PlantTypes.Find(id);

                if(plantType != null)
                {
                    plantTypesFromDb.Add(plantType);
                }
            }

            if (plantTypesFromDb.Count > 0)
            {
                SaveChanges();

                LogOperationTime(stopwatch, "PlantTypes", "queried", plantTypesFromDb);
            }

            return plantTypesFromDb;
        }

        public async Task<PlantType> Update(PlantType plantType)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            PlantType plantTypeFromDb = _context.PlantTypes.Find(plantType.Id);

            if (plantTypeFromDb == null)
            {
                plantTypeFromDb.Update(plantType);

                SaveChanges();

                LogOperationTime(stopwatch, "PlantType", "updated", plantTypeFromDb);
            }

            return plantTypeFromDb;
        }
        public async Task<List<PlantType>> UpdateByList(List<PlantType> list)
        {
            List<PlantType> plantTypesFromDb = new List<PlantType>();

            PlantType? plantTypeFromDb;

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (var plantType in list)
            {
                plantTypeFromDb = _context.PlantTypes.Find(plantType.Id);
                
                if(plantTypeFromDb != null)
                {
                    plantTypeFromDb.Update(plantType);
                    
                    plantTypesFromDb.Add(plantTypeFromDb);
                }
            }

            if(plantTypesFromDb.Count > 0)
            {
                SaveChanges();

                LogOperationTime(stopwatch, "PlantTypes", "updated", plantTypesFromDb);
            }
            return plantTypesFromDb;
        }

        public async Task<PlantType> DeleteById(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            PlantType plantTypeFromDb = _context.PlantTypes.Find(id);

            if (plantTypeFromDb == null)
            {
                _context.PlantTypes.Remove(plantTypeFromDb);

                SaveChanges();

                LogOperationTime(stopwatch, "PlantType", "deleted", plantTypeFromDb);
            }

            return plantTypeFromDb;
        }
        public async Task<List<PlantType>> DeleteAll()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<PlantType> plantTypesFromDb = _context.PlantTypes.ToList();

            if (plantTypesFromDb.Count > 0)
            {
                _context.RemoveRange(plantTypesFromDb);

                SaveChanges();

                LogOperationTime(stopwatch, "PlantTypes", "deleted", plantTypesFromDb);
            }

            return plantTypesFromDb;
        }
        public async Task<List<PlantType>> DeleteByListOfIds(List<int> ids)
        {
            List<PlantType> plantTypesFromDb =  new List<PlantType>();

            PlantType plantTypeFromDb;

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (int id in ids)
            {
                plantTypeFromDb = _context.PlantTypes.Find(id);

                plantTypesFromDb.Add(plantTypeFromDb);
             }

            if(plantTypesFromDb.Count > 0)
            {
                _context.RemoveRange(plantTypesFromDb);

                SaveChanges();

                LogOperationTime(stopwatch, "PlantTypes", "deleted", plantTypesFromDb);
            }

            return plantTypesFromDb;
        }
    }
}
