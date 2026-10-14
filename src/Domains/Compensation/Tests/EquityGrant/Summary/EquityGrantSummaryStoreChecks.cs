namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantSummaryStoreChecks
{
    public static void MemoryEquityGrantSummaryStoreFindsWhatItSaved()
    {
        var store = EquityGrantSummaryStore.NewMemoryEquityGrantSummaryStore();
        var item = EquityGrantSummary.NewEquityGrantSummary("code-766", "label-159");
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
        MemoryEquityGrantSummaryStoreFindsWhatItSaved();
    }
}
