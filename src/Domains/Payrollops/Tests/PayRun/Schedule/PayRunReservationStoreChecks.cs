namespace Acme.Workforce.Payrollops.Tests.PayRun.Schedule;

using Acme.Workforce.Payrollops.PayRun.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PayRunReservationStoreChecks
{
    public static void MemoryPayRunReservationStoreFindsWhatItSaved()
    {
        var store = PayRunReservationStore.NewMemoryPayRunReservationStore();
        var item = PayRunReservation.NewPayRunReservation("code-419", "title-768");
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
        MemoryPayRunReservationStoreFindsWhatItSaved();
    }
}
