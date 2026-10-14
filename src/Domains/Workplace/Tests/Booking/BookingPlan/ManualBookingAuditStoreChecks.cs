namespace Acme.Workforce.Workplace.Tests.Booking.BookingPlan;

using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Checks of booking plan code, run by the project's runner.</summary>
public static class ManualBookingAuditStoreChecks
{
    public static void MemoryManualBookingAuditStoreFindsWhatItSaved()
    {
        var store = ManualBookingAuditStore.NewMemoryManualBookingAuditStore();
        var item = ManualBookingAudit.NewManualBookingAudit("code-671", "title-429");
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
        MemoryManualBookingAuditStoreFindsWhatItSaved();
    }
}
