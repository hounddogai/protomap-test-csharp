namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyEngagementBudgetStoreChecks
{
    public static void MemoryLegacyEngagementBudgetStoreFindsWhatItSaved()
    {
        var store = LegacyEngagementBudgetStore.NewMemoryLegacyEngagementBudgetStore();
        var item = LegacyEngagementBudget.NewLegacyEngagementBudget("code-831", "name-592");
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
        MemoryLegacyEngagementBudgetStoreFindsWhatItSaved();
    }
}
