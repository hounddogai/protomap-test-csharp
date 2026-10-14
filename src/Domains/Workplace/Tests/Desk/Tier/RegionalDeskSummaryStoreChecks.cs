namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalDeskSummaryStoreChecks
{
    public static void MemoryRegionalDeskSummaryStoreFindsWhatItSaved()
    {
        var store = RegionalDeskSummaryStore.NewMemoryRegionalDeskSummaryStore();
        var item = RegionalDeskSummary.NewRegionalDeskSummary("code-286", "title-923");
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
        MemoryRegionalDeskSummaryStoreFindsWhatItSaved();
    }
}
