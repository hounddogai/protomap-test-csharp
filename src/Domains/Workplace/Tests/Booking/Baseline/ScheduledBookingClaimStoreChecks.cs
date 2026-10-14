namespace Acme.Workforce.Workplace.Tests.Booking.Baseline;

using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledBookingClaimStoreChecks
{
    public static void MemoryScheduledBookingClaimStoreFindsWhatItSaved()
    {
        var store = ScheduledBookingClaimStore.NewMemoryScheduledBookingClaimStore();
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("code-857", "title-716");
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
        MemoryScheduledBookingClaimStoreFindsWhatItSaved();
    }
}
