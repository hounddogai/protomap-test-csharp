namespace Acme.Workforce.Contractors.Tests.Rate.Budget;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RateBudgetStoreChecks
{
    public static void MemoryRateBudgetStoreFindsWhatItSaved()
    {
        var store = RateBudgetStore.NewMemoryRateBudgetStore();
        var item = RateBudget.NewRateBudget("code-20", "name-133");
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
        MemoryRateBudgetStoreFindsWhatItSaved();
    }
}
