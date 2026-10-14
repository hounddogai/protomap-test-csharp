namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledAccessBadgeMetricStoreChecks
{
    public static void MemoryScheduledAccessBadgeMetricStoreFindsWhatItSaved()
    {
        var store = ScheduledAccessBadgeMetricStore.NewMemoryScheduledAccessBadgeMetricStore();
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("code-863", "name-726");
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
        MemoryScheduledAccessBadgeMetricStoreFindsWhatItSaved();
    }
}
