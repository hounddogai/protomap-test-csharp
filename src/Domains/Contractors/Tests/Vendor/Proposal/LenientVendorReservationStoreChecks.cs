namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientVendorReservationStoreChecks
{
    public static void MemoryLenientVendorReservationStoreFindsWhatItSaved()
    {
        var store = LenientVendorReservationStore.NewMemoryLenientVendorReservationStore();
        var item = LenientVendorReservation.NewLenientVendorReservation("code-153", "name-378");
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
        MemoryLenientVendorReservationStoreFindsWhatItSaved();
    }
}
