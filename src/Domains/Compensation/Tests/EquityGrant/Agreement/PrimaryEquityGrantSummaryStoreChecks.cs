namespace Acme.Workforce.Compensation.Tests.EquityGrant.Agreement;

using Acme.Workforce.Compensation.EquityGrant.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PrimaryEquityGrantSummaryStoreChecks
{
    public static void MemoryPrimaryEquityGrantSummaryStoreFindsWhatItSaved()
    {
        var store = PrimaryEquityGrantSummaryStore.NewMemoryPrimaryEquityGrantSummaryStore();
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("code-262", "title-548");
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
        MemoryPrimaryEquityGrantSummaryStoreFindsWhatItSaved();
    }
}
