using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reinsurance.Core;
using Reinsurance.Core.Data;

namespace Reinsurance.Tests.Fakes
{
    public sealed class FakeRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly List<T> _items;
        private int _nextId;

        public FakeRepository(IEnumerable<T> items = null)
        {
            _items = items == null ? new List<T>() : items.ToList();
            _nextId = _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;
        }

        public IQueryable<T> Table { get { return _items.AsQueryable(); } }
        public IQueryable<T> TableUntracked { get { return _items.AsQueryable(); } }
        public IDbContext Context { get { return null; } }
        public bool? AutoCommitEnabled { get; set; } = true;

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public T GetById(object id)
        {
            var value = Convert.ToInt32(id);
            return _items.FirstOrDefault(x => x.Id == value);
        }

        public Task<T> GetByIdAsync(object id)
        {
            return Task.FromResult(GetById(id));
        }

        public void Insert(T entity)
        {
            if (entity.Id == 0)
                entity.Id = _nextId++;
            _items.Add(entity);
        }

        public Task InsertAsync(T entity)
        {
            Insert(entity);
            return Task.FromResult(0);
        }

        public void Update(T entity)
        {
        }

        public Task UpdateAsync(T entity)
        {
            Update(entity);
            return Task.FromResult(0);
        }

        public void Delete(T entity)
        {
            _items.Remove(entity);
        }

        public Task DeleteAsync(T entity)
        {
            Delete(entity);
            return Task.FromResult(0);
        }
    }
}
