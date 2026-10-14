namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CappedMeritCycleBudgetStoreChecks
{
    public static void MemoryCappedMeritCycleBudgetStoreFindsWhatItSaved()
    {
        var store = CappedMeritCycleBudgetStore.NewMemoryCappedMeritCycleBudgetStore();
        var item = CappedMeritCycleBudget.NewCappedMeritCycleBudget("code-260", "name-872");
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
        MemoryCappedMeritCycleBudgetStoreFindsWhatItSaved();
    }
}
