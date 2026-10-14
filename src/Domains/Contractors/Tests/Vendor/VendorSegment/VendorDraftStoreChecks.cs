namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorDraftStoreChecks
{
    public static void MemoryVendorDraftStoreFindsWhatItSaved()
    {
        var store = VendorDraftStore.NewMemoryVendorDraftStore();
        var item = VendorDraft.NewVendorDraft("code-548", "title-748");
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
        MemoryVendorDraftStoreFindsWhatItSaved();
    }
}
