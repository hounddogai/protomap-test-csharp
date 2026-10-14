namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class StrictVendorRequestStoreChecks
{
    public static void MemoryStrictVendorRequestStoreFindsWhatItSaved()
    {
        var store = StrictVendorRequestStore.NewMemoryStrictVendorRequestStore();
        var item = StrictVendorRequest.NewStrictVendorRequest("code-706", "title-187");
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
        MemoryStrictVendorRequestStoreFindsWhatItSaved();
    }
}
