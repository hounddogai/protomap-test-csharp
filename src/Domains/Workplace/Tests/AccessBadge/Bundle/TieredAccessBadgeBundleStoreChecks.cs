namespace Acme.Workforce.Workplace.Tests.AccessBadge.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccessBadgeBundleStoreChecks
{
    public static void MemoryTieredAccessBadgeBundleStoreFindsWhatItSaved()
    {
        var store = TieredAccessBadgeBundleStore.NewMemoryTieredAccessBadgeBundleStore();
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("code-18", "title-43");
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
        MemoryTieredAccessBadgeBundleStoreFindsWhatItSaved();
    }
}
