namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionAdjustmentStoreChecks
{
    public static void MemoryDeductionAdjustmentStoreFindsWhatItSaved()
    {
        var store = DeductionAdjustmentStore.NewMemoryDeductionAdjustmentStore();
        var item = DeductionAdjustment.NewDeductionAdjustment("code-251", "title-745");
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
        MemoryDeductionAdjustmentStoreFindsWhatItSaved();
    }
}
