namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingAgreementStoreChecks
{
    public static void MemoryBookingAgreementStoreFindsWhatItSaved()
    {
        var store = BookingAgreementStore.NewMemoryBookingAgreementStore();
        var item = BookingAgreement.NewBookingAgreement("code-870", "name-657");
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
        MemoryBookingAgreementStoreFindsWhatItSaved();
    }
}
