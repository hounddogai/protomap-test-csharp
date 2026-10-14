namespace Acme.Workforce.Workplace.Tests.Booking.Allocation;

using Acme.Workforce.Workplace.Booking.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ManualBookingAllocationStoreChecks
{
    public static void MemoryManualBookingAllocationStoreFindsWhatItSaved()
    {
        var store = ManualBookingAllocationStore.NewMemoryManualBookingAllocationStore();
        var item = ManualBookingAllocation.NewManualBookingAllocation("code-753", "label-243");
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
        MemoryManualBookingAllocationStoreFindsWhatItSaved();
    }
}
