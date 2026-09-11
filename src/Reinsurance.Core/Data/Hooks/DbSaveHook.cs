namespace Reinsurance.Core.Data.Hooks
{
    public abstract class DbSaveHook<TEntity> : IDbSaveHook where TEntity : class
    {
        public virtual void OnBeforeSave(IHookedEntity entry) { }
        public virtual void OnAfterSave(IHookedEntity entry) { }
        public virtual void OnBeforeSaveCompleted() { }
        public virtual void OnAfterSaveCompleted() { }
    }
}
