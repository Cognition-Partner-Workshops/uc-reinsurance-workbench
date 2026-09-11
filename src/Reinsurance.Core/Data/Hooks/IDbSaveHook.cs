namespace Reinsurance.Core.Data.Hooks
{
    public interface IDbSaveHook
    {
        void OnBeforeSave(IHookedEntity entry);
        void OnAfterSave(IHookedEntity entry);
        void OnBeforeSaveCompleted();
        void OnAfterSaveCompleted();
    }
}
