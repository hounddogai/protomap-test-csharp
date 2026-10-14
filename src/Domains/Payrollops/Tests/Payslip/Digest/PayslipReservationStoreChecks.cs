namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipReservationStoreChecks
{
    public static void MemoryPayslipReservationStoreFindsWhatItSaved()
    {
        var store = PayslipReservationStore.NewMemoryPayslipReservationStore();
        var item = PayslipReservation.NewPayslipReservation("code-666", "name-430");
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
        MemoryPayslipReservationStoreFindsWhatItSaved();
    }
}
