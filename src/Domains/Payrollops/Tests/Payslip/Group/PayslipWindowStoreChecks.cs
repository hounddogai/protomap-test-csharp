namespace Acme.Workforce.Payrollops.Tests.Payslip.Group;

using Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PayslipWindowStoreChecks
{
    public static void MemoryPayslipWindowStoreFindsWhatItSaved()
    {
        var store = PayslipWindowStore.NewMemoryPayslipWindowStore();
        var item = PayslipWindow.NewPayslipWindow("code-636", "name-413");
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
        MemoryPayslipWindowStoreFindsWhatItSaved();
    }
}
