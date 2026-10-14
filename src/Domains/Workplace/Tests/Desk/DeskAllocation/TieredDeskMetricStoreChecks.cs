namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class TieredDeskMetricStoreChecks
{
    public static void MemoryTieredDeskMetricStoreFindsWhatItSaved()
    {
        var store = TieredDeskMetricStore.NewMemoryTieredDeskMetricStore();
        var item = TieredDeskMetric.NewTieredDeskMetric("code-127", "name-576");
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
        MemoryTieredDeskMetricStoreFindsWhatItSaved();
    }
}
