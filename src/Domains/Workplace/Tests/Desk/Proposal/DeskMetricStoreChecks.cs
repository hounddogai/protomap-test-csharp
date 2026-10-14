namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskMetricStoreChecks
{
    public static void MemoryDeskMetricStoreFindsWhatItSaved()
    {
        var store = DeskMetricStore.NewMemoryDeskMetricStore();
        var item = DeskMetric.NewDeskMetric("code-31", "name-846");
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
        MemoryDeskMetricStoreFindsWhatItSaved();
    }
}
