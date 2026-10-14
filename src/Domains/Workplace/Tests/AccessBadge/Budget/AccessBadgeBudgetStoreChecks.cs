namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeBudgetStoreChecks
{
    public static void MemoryAccessBadgeBudgetStoreFindsWhatItSaved()
    {
        var store = AccessBadgeBudgetStore.NewMemoryAccessBadgeBudgetStore();
        var item = AccessBadgeBudget.NewAccessBadgeBudget("code-754", "name-141");
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
        MemoryAccessBadgeBudgetStoreFindsWhatItSaved();
    }
}
