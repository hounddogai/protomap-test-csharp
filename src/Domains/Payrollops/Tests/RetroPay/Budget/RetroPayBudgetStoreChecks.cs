namespace Acme.Workforce.Payrollops.Tests.RetroPay.Budget;

using Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RetroPayBudgetStoreChecks
{
    public static void MemoryRetroPayBudgetStoreFindsWhatItSaved()
    {
        var store = RetroPayBudgetStore.NewMemoryRetroPayBudgetStore();
        var item = RetroPayBudget.NewRetroPayBudget("code-911", "label-202");
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
        MemoryRetroPayBudgetStoreFindsWhatItSaved();
    }
}
