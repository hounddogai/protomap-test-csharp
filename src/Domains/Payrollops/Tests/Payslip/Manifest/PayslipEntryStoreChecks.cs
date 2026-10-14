namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PayslipEntryStoreChecks
{
    public static void MemoryPayslipEntryStoreFindsWhatItSaved()
    {
        var store = PayslipEntryStore.NewMemoryPayslipEntryStore();
        var item = PayslipEntry.NewPayslipEntry("code-723", "title-119");
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
        MemoryPayslipEntryStoreFindsWhatItSaved();
    }
}
