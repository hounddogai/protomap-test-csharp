namespace Acme.Workforce.Orgdesign.Tests.CostModel.Snapshot;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class CostModelManifestStoreChecks
{
    public static void MemoryCostModelManifestStoreFindsWhatItSaved()
    {
        var store = CostModelManifestStore.NewMemoryCostModelManifestStore();
        var item = CostModelManifest.NewCostModelManifest("code-416", "name-464");
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
        MemoryCostModelManifestStoreFindsWhatItSaved();
    }
}
