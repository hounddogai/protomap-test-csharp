namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class GlobalDeductionBaselineStoreChecks
{
    public static void MemoryGlobalDeductionBaselineStoreFindsWhatItSaved()
    {
        var store = GlobalDeductionBaselineStore.NewMemoryGlobalDeductionBaselineStore();
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("code-976", "label-681");
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
        MemoryGlobalDeductionBaselineStoreFindsWhatItSaved();
    }
}
