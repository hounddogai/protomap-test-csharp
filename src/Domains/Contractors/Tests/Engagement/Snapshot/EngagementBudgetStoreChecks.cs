namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementBudgetStoreChecks
{
    public static void MemoryEngagementBudgetStoreFindsWhatItSaved()
    {
        var store = EngagementBudgetStore.NewMemoryEngagementBudgetStore();
        var item = EngagementBudget.NewEngagementBudget("code-954", "title-821");
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
        MemoryEngagementBudgetStoreFindsWhatItSaved();
    }
}
