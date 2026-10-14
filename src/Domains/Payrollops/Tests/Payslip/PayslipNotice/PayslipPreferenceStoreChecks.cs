namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class PayslipPreferenceStoreChecks
{
    public static void MemoryPayslipPreferenceStoreFindsWhatItSaved()
    {
        var store = PayslipPreferenceStore.NewMemoryPayslipPreferenceStore();
        var item = PayslipPreference.NewPayslipPreference("code-677", "name-580");
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
        MemoryPayslipPreferenceStoreFindsWhatItSaved();
    }
}
