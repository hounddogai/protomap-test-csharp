namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class AdaptivePayslipClaimStoreChecks
{
    public static void MemoryAdaptivePayslipClaimStoreFindsWhatItSaved()
    {
        var store = AdaptivePayslipClaimStore.NewMemoryAdaptivePayslipClaimStore();
        var item = AdaptivePayslipClaim.NewAdaptivePayslipClaim("code-291", "label-749");
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
        MemoryAdaptivePayslipClaimStoreFindsWhatItSaved();
    }
}
