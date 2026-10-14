namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorScheduleStoreChecks
{
    public static void MemoryVendorScheduleStoreFindsWhatItSaved()
    {
        var store = VendorScheduleStore.NewMemoryVendorScheduleStore();
        var item = VendorSchedule.NewVendorSchedule("code-94", "name-598");
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
        MemoryVendorScheduleStoreFindsWhatItSaved();
    }
}
