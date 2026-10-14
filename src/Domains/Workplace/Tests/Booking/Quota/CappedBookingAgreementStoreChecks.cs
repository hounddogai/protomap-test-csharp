namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CappedBookingAgreementStoreChecks
{
    public static void MemoryCappedBookingAgreementStoreFindsWhatItSaved()
    {
        var store = CappedBookingAgreementStore.NewMemoryCappedBookingAgreementStore();
        var item = CappedBookingAgreement.NewCappedBookingAgreement("code-442", "name-725");
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
        MemoryCappedBookingAgreementStoreFindsWhatItSaved();
    }
}
