namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdjustmentManifestStoreChecks
{
    public static void MemoryAdjustmentManifestStoreFindsWhatItSaved()
    {
        var store = AdjustmentManifestStore.NewMemoryAdjustmentManifestStore();
        var item = AdjustmentManifest.NewAdjustmentManifest("code-501", "title-865");
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
        MemoryAdjustmentManifestStoreFindsWhatItSaved();
    }
}
