namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorRequestStoreChecks
{
    public static void MemoryVendorRequestStoreFindsWhatItSaved()
    {
        var store = VendorRequestStore.NewMemoryVendorRequestStore();
        var item = VendorRequest.NewVendorRequest("code-556", "label-912");
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
        MemoryVendorRequestStoreFindsWhatItSaved();
    }
}
