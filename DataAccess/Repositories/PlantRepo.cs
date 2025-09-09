using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;
using Serilog;
using DataAccess;
using Microsoft.IdentityModel.Tokens;
using LoggingService;

namespace DataAccess.Repositories
{
    public class PlantRepo(ApiContext context, ILoggingService logger) : BaseRepo(context, logger), IRepo<Plant>
    {
        /// <summary>
        /// Creates a new plant entity in the data base.
        /// </summary>
        /// <param name="plant"></param>
        /// <returns>An object of the created palant entity.</returns>
        public async Task<Plant> Create(Plant plant)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            await _context.Plants.AddAsync(plant);

            await SaveChanges();

            LogOperationTime(stopwatch, "Plant", "created", plant);

            return plant;
        }
        public async Task<List<Plant>> CreateByList(List<Plant> plants)
        {
            if (!plants.IsNullOrEmpty())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                foreach (var plant in plants)
                {
                    await _context.Plants.AddAsync(plant);
                }

                await SaveChanges();

                LogOperationTime(stopwatch, "Plants", "created", plants);
            }

            return plants;
        }

        /// <summary>
        /// Fetches a plant entity from the database, depending on the plant id.
        /// </summary>
        /// <param name="id">The unique identifier of the plant entity in the data table.</param>
        /// <returns>An object of the queried palant entity.</returns>
        public async Task<Plant> GetById(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Plant? plantFromDb = await _context.Plants.FindAsync(id);

            if(plantFromDb != null)
            {
                LogOperationTime(stopwatch, "Plant", "queried", plantFromDb);
            }            

            return plantFromDb;
        }
        public async Task<List<Plant>> GetByListOfIds(List<int> ids)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            List<Plant> plantsFromDb = new List<Plant>();
            
            Plant? plantFromDb;
            
            foreach (int id in ids)
            {
                plantFromDb = await _context.Plants.FindAsync(id);

                if(plantFromDb != null)
                {
                    plantsFromDb.Add(plantFromDb);
                }
            }

            if (plantsFromDb.Count > 0)
            {
                LogOperationTime(stopwatch, "Plants", "queried", plantsFromDb);
            }

            return plantsFromDb;
        }

        public async Task<List<Plant>> GetAll()
        {
            Stopwatch stopwatch= Stopwatch.StartNew();

            List<Plant> plantsFromDb = _context.Plants.ToList();

            if(plantsFromDb.Count > 0)
            {
                LogOperationTime(stopwatch, "Plants", "queried", plantsFromDb);
            }

            return plantsFromDb;
        }

        /// <summary>
        /// Updates plant entity in the database.
        /// </summary>
        /// <param name="plant">The plant entity including the new parameters for the update.</param>
        /// <returns>An object of the updated palant entity.</returns>
        public async Task<Plant> Update(Plant plant)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Plant plantFromDb = _context.Plants.Find(plant.Id);
            plantFromDb.UpdatePlant(plant);

            SaveChanges();

            LogOperationTime(stopwatch, "Plant", "updated", plantFromDb);

            return plantFromDb;
        }
        public async Task<List<Plant>> UpdateByList(List<Plant> plants)
        {
            List<Plant> plantsFromDb = new List<Plant>();

            Plant? plantFromDb;

            Stopwatch stopwatch= Stopwatch.StartNew();

            foreach (Plant plant in plants)
            {
                plantFromDb = _context.Plants.Find(plant.Id);

                if (plantFromDb != null)
                {
                    plantFromDb.UpdatePlant(plant);
                    plantsFromDb.Add(plantFromDb);
                }
            }

            if(plantsFromDb.Count > 0)
            {
                SaveChanges();
                LogOperationTime(stopwatch, "Plants", "updated", plantsFromDb);
            }

            return plantsFromDb;
        }

        /// <summary>
        /// Deletes plant entity in the datalake.
        /// </summary>
        /// <param name="id">The unique identifier of the plant entity in the data tabel.</param>
        /// <returns>An object of the deleted palant entity.</returns>
        public async Task<Plant> DeleteById(int id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Plant plantFromDb = _context.Plants.Find(id);
            if (plantFromDb != null)
            {
                _context.Plants.Remove(plantFromDb);

                SaveChanges();
            }

            LogOperationTime(stopwatch, "Plant", "deleted", plantFromDb);

            return plantFromDb;
        }
        public async Task<List<Plant>> DeleteByListOfIds(List<int> ids)
        {
            List<Plant> plantsFromDb = new List<Plant>();

            Plant plantFromDb;

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (int id in ids)
            {
                plantFromDb = _context.Plants.Find(id);

                if (plantFromDb != null)
                {
                    plantsFromDb.Add(plantFromDb);
                }

                if (plantsFromDb.Count > 0)
                {
                    _context.Plants.RemoveRange(plantsFromDb);

                    SaveChanges();

                    LogOperationTime(stopwatch, "Plants", "deleted", plantsFromDb);
                }

            }

            return plantsFromDb;
        }
        public async Task<List<Plant>> DeleteAll()
        {
            List<Plant> plantsFromDb = _context.Plants.ToList();

            Stopwatch stopwatch = Stopwatch.StartNew();

            if (plantsFromDb.Count > 0)
            {
                _context.Plants.RemoveRange(plantsFromDb);

                SaveChanges();

                LogOperationTime(stopwatch, "Plants", "deleted", plantsFromDb);
            }

            return plantsFromDb;
        }
    }
}
