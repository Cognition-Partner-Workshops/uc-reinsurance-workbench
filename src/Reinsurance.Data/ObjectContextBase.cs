using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Reinsurance.Core;
using Reinsurance.Core.Data;
using Reinsurance.Core.Data.Hooks;
using CoreEntityState = Reinsurance.Core.Data.EntityState;

namespace Reinsurance.Data
{
    public abstract class ObjectContextBase : DbContext, IDbContext
    {
        private readonly DefaultDbHookHandler _hookHandler;

        protected ObjectContextBase(string nameOrConnectionString, IEnumerable<IDbSaveHook> hooks)
            : base(nameOrConnectionString)
        {
            _hookHandler = new DefaultDbHookHandler(hooks);
            AutoCommitEnabled = true;
        }

        public bool AutoCommitEnabled { get; set; }
        public bool ForceNoTracking { get; set; }

        public System.Data.Entity.DbContextTransaction BeginTransaction()
        {
            return Database.BeginTransaction();
        }

        public DbEntityEntry Entry(BaseEntity entity)
        {
            return base.Entry(entity);
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlCommand(sql, parameters);
        }

        public void DetectChanges()
        {
            ChangeTracker.DetectChanges();
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            var entries = HookedEntries().ToList();
            _hookHandler.Before(entries);
            var result = base.SaveChanges();
            _hookHandler.After(entries);
            return result;
        }

        public override async Task<int> SaveChangesAsync()
        {
            ChangeTracker.DetectChanges();
            var entries = HookedEntries().ToList();
            _hookHandler.Before(entries);
            var result = await base.SaveChangesAsync().ConfigureAwait(false);
            _hookHandler.After(entries);
            return result;
        }

        private IEnumerable<IHookedEntity> HookedEntries()
        {
            return ChangeTracker.Entries<BaseEntity>()
                .Where(x => x.State != System.Data.Entity.EntityState.Unchanged)
                .Select(x => new HookedEntity(this, x));
        }

        private sealed class HookedEntity : IHookedEntity
        {
            private readonly DbEntityEntry _entry;
            private readonly CoreEntityState _initialState;

            public HookedEntity(DbContext context, DbEntityEntry entry)
            {
                ContextType = context.GetType();
                _entry = entry;
                _initialState = ToState(entry.State);
            }

            public Type ContextType { get; private set; }
            public DbEntityEntry Entry { get { return _entry; } }
            public BaseEntity Entity { get { return (BaseEntity)_entry.Entity; } }
            public CoreEntityState InitialState { get { return _initialState; } }
            public CoreEntityState State
            {
                get { return ToState(_entry.State); }
                set { _entry.State = (System.Data.Entity.EntityState)value; }
            }

            public bool IsPropertyModified(string propertyName)
            {
                return _entry.Property(propertyName).IsModified;
            }

            private static CoreEntityState ToState(System.Data.Entity.EntityState state)
            {
                return (CoreEntityState)(1 << (int)state);
            }
        }
    }
}
