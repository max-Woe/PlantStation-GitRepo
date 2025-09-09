using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Repositories;

namespace DataAccess.Interfaces
{
    public interface IRepo<T> where T : class
    {
        Task<T> Create(T entity);
        Task<List<T>>CreateByList(List<T> entities);
        
        Task<T?> GetById(int id);
        Task<List<T>> GetByListOfIds(List<int> ids);
        Task<List<T>> GetAll();

        Task<T?> Update(T entity);
        Task<List<T>> UpdateByList(List<T> list);

        Task<T?> DeleteById(int id);
        Task<List<T>> DeleteByListOfIds(List<int> ids);
        Task<List<T>> DeleteAll();

        Task SaveChanges();
    }
}
