namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingDeductionAdjustmentStoreChecks
{
    public static void MemoryRollingDeductionAdjustmentStoreFindsWhatItSaved()
    {
        var store = RollingDeductionAdjustmentStore.NewMemoryRollingDeductionAdjustmentStore();
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("code-194", "name-82");
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
        MemoryRollingDeductionAdjustmentStoreFindsWhatItSaved();
    }
}
