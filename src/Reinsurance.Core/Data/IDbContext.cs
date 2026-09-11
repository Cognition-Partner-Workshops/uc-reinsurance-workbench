using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace Reinsurance.Core.Data
{
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbEntityEntry Entry(BaseEntity entity);
        int SaveChanges();
        Task<int> SaveChangesAsync();
        bool AutoCommitEnabled { get; set; }
        bool ForceNoTracking { get; set; }
        int ExecuteSqlCommand(string sql, params object[] parameters);
        void DetectChanges();
        DbContextTransaction BeginTransaction();
    }
}
