namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyCostModelManifestStoreChecks
{
    public static void MemoryLegacyCostModelManifestStoreFindsWhatItSaved()
    {
        var store = LegacyCostModelManifestStore.NewMemoryLegacyCostModelManifestStore();
        var item = LegacyCostModelManifest.NewLegacyCostModelManifest("code-910", "title-805");
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
        MemoryLegacyCostModelManifestStoreFindsWhatItSaved();
    }
}
