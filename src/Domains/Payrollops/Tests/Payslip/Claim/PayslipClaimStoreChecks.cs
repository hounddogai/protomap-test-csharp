namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipClaimStoreChecks
{
    public static void MemoryPayslipClaimStoreFindsWhatItSaved()
    {
        var store = PayslipClaimStore.NewMemoryPayslipClaimStore();
        var item = PayslipClaim.NewPayslipClaim("code-460", "label-113");
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
        MemoryPayslipClaimStoreFindsWhatItSaved();
    }
}
