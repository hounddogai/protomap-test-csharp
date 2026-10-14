namespace Acme.Workforce.Contractors.Tests.Vendor.Assignment;

using Acme.Workforce.Contractors.Vendor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalVendorSlotStoreChecks
{
    public static void MemoryGlobalVendorSlotStoreFindsWhatItSaved()
    {
        var store = GlobalVendorSlotStore.NewMemoryGlobalVendorSlotStore();
        var item = GlobalVendorSlot.NewGlobalVendorSlot("code-819", "label-87");
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
        MemoryGlobalVendorSlotStoreFindsWhatItSaved();
    }
}
