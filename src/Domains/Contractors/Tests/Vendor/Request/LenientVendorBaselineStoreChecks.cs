namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBaselineStoreChecks
{
    public static void MemoryLenientVendorBaselineStoreFindsWhatItSaved()
    {
        var store = LenientVendorBaselineStore.NewMemoryLenientVendorBaselineStore();
        var item = LenientVendorBaseline.NewLenientVendorBaseline("code-225", "title-22");
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
        MemoryLenientVendorBaselineStoreFindsWhatItSaved();
    }
}
