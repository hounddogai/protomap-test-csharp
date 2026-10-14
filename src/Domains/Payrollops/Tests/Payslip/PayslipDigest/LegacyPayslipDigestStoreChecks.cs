namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipDigest;

using Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>Checks of payslip digest code, run by the project's runner.</summary>
public static class LegacyPayslipDigestStoreChecks
{
    public static void MemoryLegacyPayslipDigestStoreFindsWhatItSaved()
    {
        var store = LegacyPayslipDigestStore.NewMemoryLegacyPayslipDigestStore();
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("code-876", "title-810");
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
        MemoryLegacyPayslipDigestStoreFindsWhatItSaved();
    }
}
