namespace DataAccess.Interfaces
{
    /// <summary>
    /// Defines the generic contract for a Repository responsible for performing basic CRUD operations
    /// (Create, Read, Update, Delete) on a specific entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity managed by the repository. Must be a class (<c>class</c>).</typeparam>
    public interface IRepo<T> where T : class
    {
        // ------------------------------------
        // C - CREATE Operations
        // ------------------------------------

        /// <summary>
        /// Adds a single entity to the database.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A Task that returns the stored entity with its updated ID, or <c>null</c> if the operation fails.</returns>
        Task<T?> Create(T entity);

        /// <summary>
        /// Adds a list of entities to the database.
        /// </summary>
        /// <param name="entities">The list of entities to add.</param>
        /// <returns>A Task that returns the list of stored entities. Returns an empty list on failure.</returns>
        Task<List<T>> CreateByList(List<T> entities);


        // ------------------------------------
        // R - READ Operations
        // ------------------------------------

        /// <summary>
        /// Retrieves an entity by its primary key ID.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>A Task that returns the found entity or <c>null</c> if no entity with the given ID exists.</returns>
        Task<T?> GetById(int id);

        /// <summary>
        /// Retrieves a list of entities based on a list of IDs.
        /// </summary>
        /// <param name="ids">The list of IDs whose entities should be retrieved.</param>
        /// <returns>A Task that returns a list of the entities found. Entities with non-existent IDs are ignored.</returns>
        Task<List<T>> GetByListOfIds(List<int> ids);

        /// <summary>
        /// Retrieves all entities of this type from the database.
        /// </summary>
        /// <returns>A Task that returns a list of all entities. Returns an empty list on failure.</returns>
        Task<List<T>> GetAll();


        // ------------------------------------
        // U - UPDATE Operations
        // ------------------------------------

        /// <summary>
        /// Updates an existing entity in the database.
        /// </summary>
        /// <param name="entity">The entity to update (must contain a valid ID).</param>
        /// <returns>A Task that returns the updated entity or <c>null</c> if the entity does not exist or the update fails.</returns>
        Task<T?> Update(T entity);

        /// <summary>
        /// Updates a list of entities in the database.
        /// </summary>
        /// <param name="list">The list of entities to update (must contain valid IDs).</param>
        /// <returns>A Task that returns a list of the successfully updated entities.</returns>
        Task<List<T>> UpdateByList(List<T> list);


        // ------------------------------------
        // D - DELETE Operations
        // ------------------------------------

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>A Task that returns the deleted entity or <c>null</c> if the entity does not exist or deletion fails.</returns>
        Task<T?> Delete(int id);

        /// <summary>
        /// Deletes a list of entities based on a list of IDs.
        /// </summary>
        /// <param name="ids">The list of IDs whose entities should be deleted.</param>
        /// <returns>A Task that returns a list of the entities that were actually deleted.</returns>
        Task<List<T>> DeleteByListOfIds(List<int> ids);

        /// <summary>
        /// Deletes all entities of this type from the database.
        /// </summary>
        /// <returns>A Task that returns a list of all deleted entities.</returns>
        Task<List<T>> DeleteAll();


        // ------------------------------------
        // Utility
        // ------------------------------------

        /// <summary>
        /// Saves all pending changes made in the context after CRUD operations to the database.
        /// </summary>
        /// <returns>A Task that completes upon saving changes.</returns>
        Task SaveChanges();
    }
}