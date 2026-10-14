namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class AdaptivePayslipReviewStoreChecks
{
    public static void MemoryAdaptivePayslipReviewStoreFindsWhatItSaved()
    {
        var store = AdaptivePayslipReviewStore.NewMemoryAdaptivePayslipReviewStore();
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("code-751", "name-822");
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
        MemoryAdaptivePayslipReviewStoreFindsWhatItSaved();
    }
}
