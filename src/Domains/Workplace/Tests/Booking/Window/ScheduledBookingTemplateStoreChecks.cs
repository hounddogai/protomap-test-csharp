namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledBookingTemplateStoreChecks
{
    public static void MemoryScheduledBookingTemplateStoreFindsWhatItSaved()
    {
        var store = ScheduledBookingTemplateStore.NewMemoryScheduledBookingTemplateStore();
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("code-651", "name-222");
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
        MemoryScheduledBookingTemplateStoreFindsWhatItSaved();
    }
}
