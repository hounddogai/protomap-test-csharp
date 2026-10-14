namespace Acme.Workforce.Payrollops.Tests.Deduction.Budget;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeductionBudgetStoreChecks
{
    public static void MemoryDeductionBudgetStoreFindsWhatItSaved()
    {
        var store = DeductionBudgetStore.NewMemoryDeductionBudgetStore();
        var item = DeductionBudget.NewDeductionBudget("code-598", "title-974");
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
        MemoryDeductionBudgetStoreFindsWhatItSaved();
    }
}
