namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMetricStoreChecks
{
    public static void MemoryAccessBadgeMetricStoreFindsWhatItSaved()
    {
        var store = AccessBadgeMetricStore.NewMemoryAccessBadgeMetricStore();
        var item = AccessBadgeMetric.NewAccessBadgeMetric("code-173", "label-849");
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
        MemoryAccessBadgeMetricStoreFindsWhatItSaved();
    }
}
