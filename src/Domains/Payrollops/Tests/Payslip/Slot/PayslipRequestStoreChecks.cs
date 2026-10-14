namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipRequestStoreChecks
{
    public static void MemoryPayslipRequestStoreFindsWhatItSaved()
    {
        var store = PayslipRequestStore.NewMemoryPayslipRequestStore();
        var item = PayslipRequest.NewPayslipRequest("code-917", "title-894");
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
        MemoryPayslipRequestStoreFindsWhatItSaved();
    }
}
