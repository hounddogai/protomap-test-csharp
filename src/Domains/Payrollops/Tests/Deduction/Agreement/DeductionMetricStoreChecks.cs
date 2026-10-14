namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionMetricStoreChecks
{
    public static void MemoryDeductionMetricStoreFindsWhatItSaved()
    {
        var store = DeductionMetricStore.NewMemoryDeductionMetricStore();
        var item = DeductionMetric.NewDeductionMetric("code-994", "name-342");
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
        MemoryDeductionMetricStoreFindsWhatItSaved();
    }
}
