namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalEngagementBudgetStoreChecks
{
    public static void MemoryRegionalEngagementBudgetStoreFindsWhatItSaved()
    {
        var store = RegionalEngagementBudgetStore.NewMemoryRegionalEngagementBudgetStore();
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("code-227", "label-259");
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
        MemoryRegionalEngagementBudgetStoreFindsWhatItSaved();
    }
}
