namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class AdaptiveDeductionBaselineStoreChecks
{
    public static void MemoryAdaptiveDeductionBaselineStoreFindsWhatItSaved()
    {
        var store = AdaptiveDeductionBaselineStore.NewMemoryAdaptiveDeductionBaselineStore();
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("code-969", "title-124");
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
        MemoryAdaptiveDeductionBaselineStoreFindsWhatItSaved();
    }
}
