namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class CappedAccrualManifestStoreChecks
{
    public static void MemoryCappedAccrualManifestStoreFindsWhatItSaved()
    {
        var store = CappedAccrualManifestStore.NewMemoryCappedAccrualManifestStore();
        var item = CappedAccrualManifest.NewCappedAccrualManifest("code-509", "label-732");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryCappedAccrualManifestStoreFindsWhatItSaved();
    }
}
