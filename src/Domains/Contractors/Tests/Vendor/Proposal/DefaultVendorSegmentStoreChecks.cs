namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorSegmentStoreChecks
{
    public static void MemoryDefaultVendorSegmentStoreFindsWhatItSaved()
    {
        var store = DefaultVendorSegmentStore.NewMemoryDefaultVendorSegmentStore();
        var item = DefaultVendorSegment.NewDefaultVendorSegment("code-861", "label-968");
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
        MemoryDefaultVendorSegmentStoreFindsWhatItSaved();
    }
}
