namespace Acme.Workforce.Payrollops.Tests.Payslip.Profile;

using Acme.Workforce.Payrollops.Payslip.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PayslipProfileStoreChecks
{
    public static void MemoryPayslipProfileStoreFindsWhatItSaved()
    {
        var store = PayslipProfileStore.NewMemoryPayslipProfileStore();
        var item = PayslipProfile.NewPayslipProfile("code-434", "title-533");
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
        MemoryPayslipProfileStoreFindsWhatItSaved();
    }
}
