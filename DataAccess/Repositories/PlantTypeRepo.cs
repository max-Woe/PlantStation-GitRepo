using System.Diagnostics;
using DataAccess.Models;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using LoggingService;
using Microsoft.IdentityModel.Tokens;

namespace DataAccess.Repositories
{
    /// <summary>
    /// Implements the <see cref="IRepo{T}"/> contract for the <see cref="PlantType"/> entity.
    /// Provides concrete database access (CRUD) and logging functionality for plant type entities.
    /// </summary>
    public class PlantTypeRepo(ApiContext context, ILoggingService logger) : BaseRepo(context, logger), IRepo<PlantType>
    {
        // ------------------------------------
        // C - CREATE Operations
        // ------------------------------------

        /// <summary>
        /// Creates a single plant type entity in the database.
        /// </summary>
        /// <param name="plantType">The <see cref="PlantType"/> entity to be stored.</param>
        /// <returns>A Task that returns the stored <see cref="PlantType"/> entity with its updated ID, or <c>null</c> if the operation fails or the input is invalid.</returns>
        public async Task<PlantType?> Create(PlantType plantType)
        {
            if (plantType == null) 
            {
                return null;
            }
            _logger.StartTimer();

            try
            {
                await TryExecuteAsync(async () => await _context.PlantTypes.AddAsync(plantType), "AddAsync", "Create", plantType);

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Create", plantType);

                return plantType;
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
        /// Creates a list of plant type entities in the database.
        /// </summary>
        /// <param name="plantTypes">The list of <see cref="PlantType"/> entities to be stored.</param>
        /// <returns>A Task that returns the list of successfully stored <see cref="PlantType"/> entities. Returns an empty list if the input is empty or on failure.</returns>
        public async Task<List<PlantType>> CreateByList(List<PlantType> plantTypes)
        {
            if (plantTypes == null || plantTypes.Count == 0)
            {
                return new List<PlantType>();
            }

            _logger.StartTimer();

            List<PlantType> createdPlantTypes = new List<PlantType>();

            try
            {
                foreach (var plantType in plantTypes)
                {
                    await TryExecuteAsync(async () => await _context.PlantTypes.AddAsync(plantType), "AddAsync", "CreateByList", plantType);
                    createdPlantTypes.Add(plantType);
                }

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "CreateByList", plantTypes);

                return createdPlantTypes;
            }
            catch (Exception)
            {
                return new List<PlantType>();
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
        /// Retrieves a single plant type entity by its primary key ID.
        /// </summary>
        /// <param name="id">The ID of the plant type to retrieve.</param>
        /// <returns>A Task that returns the found <see cref="PlantType"/> or <c>null</c> if it does not exist or the ID is invalid.</returns>
        public async Task<PlantType> GetById(int id)
        {
            if(id<= 0)
            {
                return null;
            }

            _logger.StartTimer();

            try
            {
                PlantType? plantTypeFromContext = await TryExecuteAsync(async () => await _context.PlantTypes.FindAsync(id), "FindAsync", "GetById", null);

                if (plantTypeFromContext == null)
                {
                    return null;
                }

                return plantTypeFromContext;
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
        public async Task<List<PlantType>> GetAll()
        {
            _logger.StartTimer();

            try
            {
                List<PlantType>? plantTypesFromContext = await TryExecuteAsync(async () => await _context.PlantTypes.ToListAsync(), "ToListAsync", "GetAll", null);
                
                if (plantTypesFromContext.IsNullOrEmpty())
                {
                    return new List<PlantType>();
                }

                return plantTypesFromContext;
            }
            catch (Exception)
            {
                return new List<PlantType>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<List<PlantType>> GetByListOfIds(List<int> ids)
        {
            if(ids.IsNullOrEmpty())
            {
                return new List<PlantType>();
            }

            _logger.StartTimer();

            try
            {
                List<PlantType>? plantTypesFromContext = await TryExecuteAsync(async () => await _context.PlantTypes.Where(pt => ids.Contains(pt.Id)).ToListAsync(), "Where/ToListAsync", "GetByListOfIds", ids);
                
                if (plantTypesFromContext.IsNullOrEmpty())
                {
                    return new List<PlantType>();
                }

                return plantTypesFromContext!;
            }
            catch (Exception)
            {
                return new List<PlantType>();
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
        /// Updates an existing plant type entity in the database.
        /// </summary>
        /// <param name="plantType">The <see cref="PlantType"/> entity including the new parameters for the update (ID must be valid).</param>
        /// <returns>A Task that returns the updated <see cref="PlantType"/> entity from the database, or <c>null</c> if the entity was not found or the update fails.</returns>
        public async Task<PlantType?> Update(PlantType plantType)
        {
            if(plantType == null)
            {
                return null;
            }
            _logger.StartTimer();

            try
            {
                Plant? plantFromContext = await TryExecuteAsync(async () => await _context.Plants.FindAsync(plantType.Id), "FindAsync", "Upadate", plantType);
                if (plantFromContext == null)
                {
                    return null;
                }

                plantFromContext.UpdatePlant(plantFromContext);
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Update", plantType);

                return plantFromContext.PlantType;
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
        public async Task<List<PlantType>> UpdateByList(List<PlantType> plantTypes)
        {
            _logger.StartTimer();

            try
            {
                List<PlantType> plantTypesFromContext = new List<PlantType>();
                List<int> plantTypeIds = plantTypes.Select(pt => pt.Id).ToList();

                PlantType? plantTypeFromContext;

                plantTypesFromContext = await TryExecuteAsync(async () => await _context.PlantTypes.Where(pt => plantTypeIds.Contains(pt.Id)).ToListAsync(), "Where/ToListAsync", "UpdateByList", plantTypes);

                Dictionary<int, PlantType> plantTypeDict = plantTypes.ToDictionary(p=>p.Id, p=>p);

                foreach (var plantType in plantTypesFromContext)
                {
                    if(plantTypeDict.TryGetValue(plantType.Id, out var updatedPlantType))
                    {
                        plantType.Update(updatedPlantType);
                    }
                }

                if(plantTypesFromContext.IsNullOrEmpty())
                {
                    return new List<PlantType>();
                }

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "UpdateByList", plantTypesFromContext);

                return plantTypesFromContext;
            }
            catch (Exception)
            {
                return new List<PlantType>();
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
        /// Deletes a plant type entity from the database based on its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the plant type.</param>
        /// <returns>A Task that returns the deleted <see cref="PlantType"/> entity, or <c>null</c> if the entity was not found or deletion fails.</returns>
        public async Task<PlantType?> Delete(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            _logger.StartTimer();

            try
            {
                PlantType? plantTypeFromContext = await TryExecuteAsync(async () => await _context.PlantTypes.FindAsync(id), "FindAsync", "Delete", id);
                if (plantTypeFromContext == null)
                {
                    return null;
                }
                _context.PlantTypes.Remove(plantTypeFromContext);
                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "Delete", plantTypeFromContext);

                return plantTypeFromContext;
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
        public async Task<List<PlantType>> DeleteAll()
        {
            _logger.StartTimer();

            try
            {
                List<PlantType>? plantTypesFromContext = await TryExecuteAsync(async () => await _context.PlantTypes.ToListAsync(), "ToListAsync", "DeleteAll", null);
                if(plantTypesFromContext.IsNullOrEmpty())
                {
                    return new List<PlantType>();
                }

                await TryExecuteAsync<object>(() =>
                {
                    _context.PlantTypes.RemoveRange(plantTypesFromContext);
                    return Task.FromResult<object>(null);
                }, "RemoveRange", "DeleteByListOfIds");

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "DeleteAll", plantTypesFromContext);

                return plantTypesFromContext;
            }
            catch (Exception)
            {
                return new List<PlantType>();
            }
            finally
            {
                _logger.StopTimer();
            }
        }
        /// <inheritdoc/>
        public async Task<List<PlantType>> DeleteByListOfIds(List<int> ids)
        {
            _logger.StartTimer();

            try
            {
                List<PlantType>? plantTypesFromContext = await TryExecuteAsync(async () => await _context.PlantTypes.Where(pt => ids.Contains(pt.Id)).ToListAsync(), "Where/ToListAsync", "DeleteByListOfIds", null);

                if (plantTypesFromContext.IsNullOrEmpty())
                {
                    return new List<PlantType>();
                }


                await TryExecuteAsync<object>(() =>
                {
                    _context.PlantTypes.RemoveRange(plantTypesFromContext);
                    return Task.FromResult<object>(null);
                }, "RemoveRange", "DeleteByListOfIds");

                await TryExecuteAsync(async () => await _context.SaveChangesAsync(), "SaveChangesAsync", "DeleteByListOfIds", plantTypesFromContext);

                return plantTypesFromContext;
            }
            catch (Exception)
            {
                return new List<PlantType>();
            }
        }
    }
}
