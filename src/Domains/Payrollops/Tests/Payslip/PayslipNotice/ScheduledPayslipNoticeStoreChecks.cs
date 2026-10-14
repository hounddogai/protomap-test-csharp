namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class ScheduledPayslipNoticeStoreChecks
{
    public static void MemoryScheduledPayslipNoticeStoreFindsWhatItSaved()
    {
        var store = ScheduledPayslipNoticeStore.NewMemoryScheduledPayslipNoticeStore();
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("code-328", "name-891");
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
        MemoryScheduledPayslipNoticeStoreFindsWhatItSaved();
    }
}
