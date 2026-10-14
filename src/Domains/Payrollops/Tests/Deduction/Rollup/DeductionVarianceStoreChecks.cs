namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionVarianceStoreChecks
{
    public static void MemoryDeductionVarianceStoreFindsWhatItSaved()
    {
        var store = DeductionVarianceStore.NewMemoryDeductionVarianceStore();
        var item = DeductionVariance.NewDeductionVariance("code-902", "title-344");
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
        MemoryDeductionVarianceStoreFindsWhatItSaved();
    }
}
