namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolBudgetStoreChecks
{
    public static void MemoryPoolBudgetStoreFindsWhatItSaved()
    {
        var store = PoolBudgetStore.NewMemoryPoolBudgetStore();
        var item = PoolBudget.NewPoolBudget("code-372", "name-376");
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
        MemoryPoolBudgetStoreFindsWhatItSaved();
    }
}
