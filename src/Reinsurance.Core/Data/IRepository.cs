using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reinsurance.Core.Data
{
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> Table { get; }
        IQueryable<T> TableUntracked { get; }
        T Create();
        T GetById(object id);
        Task<T> GetByIdAsync(object id);
        void Insert(T entity);
        Task InsertAsync(T entity);
        void Update(T entity);
        Task UpdateAsync(T entity);
        void Delete(T entity);
        Task DeleteAsync(T entity);
        IDbContext Context { get; }
        bool? AutoCommitEnabled { get; set; }
    }
}
