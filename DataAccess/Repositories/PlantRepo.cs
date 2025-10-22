using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Interfaces;
using Serilog;
using DataAccess;
using Microsoft.IdentityModel.Tokens;
using LoggingService;
using System.Runtime.InteropServices;

namespace DataAccess.Repositories
{
    /// <summary>     
    /// Implements the <see cref="IRepo{T}"/> contract for the <see cref="Plant"/> entity.
    /// Provides concrete database access (CRUD) and logging for plant entities.
    /// </summary>
    public class PlantRepo(ApiContext context, ILoggingService logger) : BaseRepo(context, logger), IRepo<Plant>
    {
        // ------------------------------------
        // C - CREATE Operations
        // ------------------------------------

        /// <summary>
        /// Creates a new plant entity in the database.
        /// </summary>
        /// <param name="plant">The plant entity to be stored.</param>
        /// <returns>A Task that returns the stored <see cref="Plant"/> entity with its updated ID, or <c>null</c> if the operation fails or the input is invalid.</returns>
        public async Task<Plant?> Create(Plant plant)
        {
            if(plant == null) 
            {
                return null;
            }

            _logger.StartTimer();

            try
            {
                await TryExecuteAsync(async() => await _context.Plants.AddAsync(plant), "AddAsync", "Create", plant);
                await TryExecuteAsync(async() => await _context.SaveChangesAsync(), "SaveChanges", "Create", plant);

                return plant;
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
        public async Task<List<Plant>> CreateByList(List<Plant> plants)
        {
            if (plants.IsNullOrEmpty())
            {
                return new List<Plant>();
            }

            _logger.StartTimer();

            try
            {
                foreach (var plant in plants)
                {
                    await TryExecuteAsync(async() => await _context.Plants.AddAsync(plant), "AddAsync", "CreateByList", plant);
                }
                await TryExecuteAsync(async() => await _context.SaveChangesAsync(), "SaveChanges", "CreateByList", null);
                return plants;
            }
            catch (Exception)
            {
                return new List<Plant>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }

        // ------------------------------------
        // R - READ Operations
        // ------------------------------------

        /// <summary>
        /// Fetches a plant entity from the database, based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the plant entity.</param>
        /// <returns>A Task that returns the queried <see cref="Plant"/> entity, or <c>null</c> if it does not exist or the ID is invalid.</returns>
        public async Task<Plant?> GetById(int id)
        {
            if(id <= 0)
            {
                return null;
            }

            _logger.StartTimer();
            try
            {
                Plant? plantFromContext = await TryExecuteAsync(async () => await _context.Plants.FindAsync(id), "FindAsync", "GetById", id);
                return plantFromContext;
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
        public async Task<List<Plant>> GetByListOfIds(List<int> ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return new List<Plant>();
            }
            
            _logger.StartTimer();

            try
            {
                List<Plant> plantsFromContext = await TryExecuteAsync(async () => await _context.Plants.Where(p => ids.Contains(p.Id)).ToListAsync(), "Where-Contains-ToListAsync", "GetByListOfIds", null);
                
                if(plantsFromContext == null || plantsFromContext.Count == 0)
                {
                    return new List<Plant>();
                }
                return plantsFromContext;
            }
            catch (Exception)
            {
                return new List<Plant>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }

        /// <inheritdoc/>
        public async Task<List<Plant>> GetAll()
        {
            _logger.StartTimer();
            
            try
            {
                List<Plant>? plantsFromDb = await TryExecuteAsync(async() => await _context.Plants.ToListAsync(), "ToListAsync", "GetAll", null);

                if(plantsFromDb == null || plantsFromDb.Count == 0)
                {
                    return new List<Plant>();
                }
                return plantsFromDb;
            }
            catch (Exception)
            {
                return new List<Plant>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }


        // ------------------------------------
        // U - UPDATE Operations
        // ------------------------------------

        /// <summary>
        /// Updates an existing plant entity in the database.
        /// </summary>
        /// <param name="plant">The <see cref="Plant"/> entity including the new parameters for the update (ID must be valid).</param>
        /// <returns>A Task that returns the updated <see cref="Plant"/> entity from the database, or <c>null</c> if the plant was not found or the update fails.</returns>
        public async Task<Plant?> Update(Plant plant)
        {
            _logger.StartTimer();

            try
            {
                Plant? plantFromDb = await TryExecuteAsync(async() => await _context.Plants.FindAsync(plant.Id), "FindAsync", "Update", plant);

                if (plantFromDb == null)
                {
                    return null;
                }

                plantFromDb.UpdatePlant(plant);

                await TryExecuteAsync(async() => await _context.SaveChangesAsync(), "SaveChanges", "Update", plantFromDb);

                return plantFromDb;
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
        public async Task<List<Plant>> UpdateByList(List<Plant> plants)
        {
            if(plants.IsNullOrEmpty())
            {
                return new List<Plant>();
            }
            _logger.StartTimer();

            try
            {
                List<Plant> plantsFromDb = new List<Plant>();
                Plant? plantFromContext;

                foreach (Plant plant in plants)
                {
                    plantFromContext = await TryExecuteAsync(async () => await _context.Plants.FindAsync(plant.Id), "FindAsync", "UpdateByList", plant);

                    if (plantFromContext != null)
                    {
                        plantFromContext.UpdatePlant(plant);
                        plantsFromDb.Add(plantFromContext);
                    }
                }

                if (plantsFromDb.Count > 0)
                {
                    await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "UpdateByList", null);

                    return plantsFromDb;
                }
                else
                {
                    return new List<Plant>();
                }
            }
            catch (Exception)
            {
                return new List<Plant>();
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
        /// Deletes a plant entity from the database based on its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the plant entity.</param>
        /// <returns>A Task that returns the deleted <see cref="Plant"/> entity, or <c>null</c> if the entity was not found or deletion fails.</returns>
        public async Task<Plant?> Delete(int id)
        {
            _logger.StartTimer();

            try
            {
                Plant? plantFromContext = await TryExecuteAsync(async () => await _context.Plants.FindAsync(id), "FindAsync", "Delete", null);
                
                if (plantFromContext == null)
                {
                    return null;
                }

                _context.Plants.Remove(plantFromContext);

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChanges", "Delete", plantFromContext);

                return plantFromContext;
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
        public async Task<List<Plant>> DeleteByListOfIds(List<int> ids)
        {
            if(ids.IsNullOrEmpty())
            {
                return new List<Plant>();
            }

            _logger.StartTimer();

            try
            {
                List<Plant> plantsFromContext = new List<Plant>();
                foreach (int id in ids)
                {
                    if (id != 0)
                    {
                        Plant plantFromContext = await TryExecuteAsync(async () => await _context.Plants.FindAsync(id), "FindAsync", "DeleteByListOfIds", null);
                        if (plantFromContext == null)
                        {
                            plantsFromContext.Add(plantFromContext);
                        }
                    }                    
                }

                if (plantsFromContext.Count == 0)
                {
                    return new List<Plant>();
                }

                return plantsFromContext;
            }
            catch (Exception)
            {
                return new List<Plant>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<List<Plant>> DeleteAll()
        {
            _logger.StartTimer();

            try
            {
                List<Plant> plantsFromContext = await TryExecuteAsync(async() => await _context.Plants.ToListAsync(), "ToListAsync", "DeleteAll", null);

                if(plantsFromContext == null || plantsFromContext.Count == 0)
                {
                    return new List<Plant>();
                }

                _context.Plants.RemoveRange(plantsFromContext);

                await TryExecuteAsync(async() => await _context.SaveChangesAsync(), "SaveChanges", "DeleteAll", null);

                return plantsFromContext;
            }
            catch (Exception)
            {
                return new List<Plant>();
            }
        }
    }
}
