namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipSlotStoreChecks
{
    public static void MemoryPayslipSlotStoreFindsWhatItSaved()
    {
        var store = PayslipSlotStore.NewMemoryPayslipSlotStore();
        var item = PayslipSlot.NewPayslipSlot("code-750", "title-118");
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
        MemoryPayslipSlotStoreFindsWhatItSaved();
    }
}
