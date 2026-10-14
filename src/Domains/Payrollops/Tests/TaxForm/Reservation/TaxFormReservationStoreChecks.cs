namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TaxFormReservationStoreChecks
{
    public static void MemoryTaxFormReservationStoreFindsWhatItSaved()
    {
        var store = TaxFormReservationStore.NewMemoryTaxFormReservationStore();
        var item = TaxFormReservation.NewTaxFormReservation("code-360", "label-888");
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
        MemoryTaxFormReservationStoreFindsWhatItSaved();
    }
}
