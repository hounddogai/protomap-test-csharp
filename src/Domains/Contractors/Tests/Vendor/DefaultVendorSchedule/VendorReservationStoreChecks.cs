namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class VendorReservationStoreChecks
{
    public static void MemoryVendorReservationStoreFindsWhatItSaved()
    {
        var store = VendorReservationStore.NewMemoryVendorReservationStore();
        var item = VendorReservation.NewVendorReservation("code-622", "title-719");
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
        MemoryVendorReservationStoreFindsWhatItSaved();
    }
}
