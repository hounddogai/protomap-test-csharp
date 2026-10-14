namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class BatchedDeductionBudgetStoreChecks
{
    public static void MemoryBatchedDeductionBudgetStoreFindsWhatItSaved()
    {
        var store = BatchedDeductionBudgetStore.NewMemoryBatchedDeductionBudgetStore();
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("code-393", "label-606");
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
        MemoryBatchedDeductionBudgetStoreFindsWhatItSaved();
    }
}
