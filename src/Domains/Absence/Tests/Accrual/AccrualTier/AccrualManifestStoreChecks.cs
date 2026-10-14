namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualManifestStoreChecks
{
    public static void MemoryAccrualManifestStoreFindsWhatItSaved()
    {
        var store = AccrualManifestStore.NewMemoryAccrualManifestStore();
        var item = AccrualManifest.NewAccrualManifest("code-730", "label-511");
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
        MemoryAccrualManifestStoreFindsWhatItSaved();
    }
}
