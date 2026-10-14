namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipDigestStoreChecks
{
    public static void MemoryFallbackPayslipDigestStoreFindsWhatItSaved()
    {
        var store = FallbackPayslipDigestStore.NewMemoryFallbackPayslipDigestStore();
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("code-804", "name-775");
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
        MemoryFallbackPayslipDigestStoreFindsWhatItSaved();
    }
}
