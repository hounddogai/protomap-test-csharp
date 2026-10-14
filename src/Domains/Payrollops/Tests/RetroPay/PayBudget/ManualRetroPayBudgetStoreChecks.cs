namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayBudget;

using Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>Checks of pay budget code, run by the project's runner.</summary>
public static class ManualRetroPayBudgetStoreChecks
{
    public static void MemoryManualRetroPayBudgetStoreFindsWhatItSaved()
    {
        var store = ManualRetroPayBudgetStore.NewMemoryManualRetroPayBudgetStore();
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("code-408", "label-66");
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
        MemoryManualRetroPayBudgetStoreFindsWhatItSaved();
    }
}
