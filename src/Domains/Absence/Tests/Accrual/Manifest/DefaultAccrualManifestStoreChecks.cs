namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class DefaultAccrualManifestStoreChecks
{
    public static void MemoryDefaultAccrualManifestStoreFindsWhatItSaved()
    {
        var store = DefaultAccrualManifestStore.NewMemoryDefaultAccrualManifestStore();
        var item = DefaultAccrualManifest.NewDefaultAccrualManifest("code-120", "title-455");
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
        MemoryDefaultAccrualManifestStoreFindsWhatItSaved();
    }
}
