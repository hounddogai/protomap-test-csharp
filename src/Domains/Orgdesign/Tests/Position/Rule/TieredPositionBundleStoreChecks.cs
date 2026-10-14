namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredPositionBundleStoreChecks
{
    public static void MemoryTieredPositionBundleStoreFindsWhatItSaved()
    {
        var store = TieredPositionBundleStore.NewMemoryTieredPositionBundleStore();
        var item = TieredPositionBundle.NewTieredPositionBundle("code-28", "name-162");
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
        MemoryTieredPositionBundleStoreFindsWhatItSaved();
    }
}
