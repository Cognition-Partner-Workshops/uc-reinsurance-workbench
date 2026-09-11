using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Reinsurance.Core;
using Reinsurance.Core.Data;

namespace Reinsurance.Data
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IDbContext _context;
        private readonly DbSet<T> _set;

        public EfRepository(IDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public IQueryable<T> Table { get { return _context.ForceNoTracking ? _set.AsNoTracking() : _set; } }
        public IQueryable<T> TableUntracked { get { return _set.AsNoTracking(); } }
        public IDbContext Context { get { return _context; } }
        public bool? AutoCommitEnabled { get { return _context.AutoCommitEnabled; } set { if (value.HasValue) _context.AutoCommitEnabled = value.Value; } }

        public T Create() { return _set.Create(); }
        public T GetById(object id) { return _set.Find(id); }
        public Task<T> GetByIdAsync(object id) { return _set.FindAsync(id); }

        public void Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            _set.Add(entity);
            Commit();
        }

        public async Task InsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            _set.Add(entity);
            await CommitAsync().ConfigureAwait(false);
        }

        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            _set.Attach(entity);
            _context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
            Commit();
        }

        public async Task UpdateAsync(T entity)
        {
            UpdateWithoutCommit(entity);
            await CommitAsync().ConfigureAwait(false);
        }

        public void Delete(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            _set.Remove(entity);
            Commit();
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            _set.Remove(entity);
            await CommitAsync().ConfigureAwait(false);
        }

        private void UpdateWithoutCommit(T entity)
        {
            _set.Attach(entity);
            _context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }

        private void Commit()
        {
            if (_context.AutoCommitEnabled) _context.SaveChanges();
        }

        private Task<int> CommitAsync()
        {
            return _context.AutoCommitEnabled ? _context.SaveChangesAsync() : Task.FromResult(0);
        }
    }
}
