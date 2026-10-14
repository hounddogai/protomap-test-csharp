namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class WeightedAccessBadgeBatchStoreChecks
{
    public static void MemoryWeightedAccessBadgeBatchStoreFindsWhatItSaved()
    {
        var store = WeightedAccessBadgeBatchStore.NewMemoryWeightedAccessBadgeBatchStore();
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("code-430", "label-124");
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
        MemoryWeightedAccessBadgeBatchStoreFindsWhatItSaved();
    }
}
