namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientGarnishmentBudgetStoreChecks
{
    public static void MemoryLenientGarnishmentBudgetStoreFindsWhatItSaved()
    {
        var store = LenientGarnishmentBudgetStore.NewMemoryLenientGarnishmentBudgetStore();
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("code-250", "label-403");
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
        MemoryLenientGarnishmentBudgetStoreFindsWhatItSaved();
    }
}
