namespace Acme.Workforce.Contractors.Tests.Vendor.Schedule;

using Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledVendorScheduleStoreChecks
{
    public static void MemoryScheduledVendorScheduleStoreFindsWhatItSaved()
    {
        var store = ScheduledVendorScheduleStore.NewMemoryScheduledVendorScheduleStore();
        var item = ScheduledVendorSchedule.NewScheduledVendorSchedule("code-536", "title-856");
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
        MemoryScheduledVendorScheduleStoreFindsWhatItSaved();
    }
}
