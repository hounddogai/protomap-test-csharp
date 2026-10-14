namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class PrimaryVendorProfileStoreChecks
{
    public static void MemoryPrimaryVendorProfileStoreFindsWhatItSaved()
    {
        var store = PrimaryVendorProfileStore.NewMemoryPrimaryVendorProfileStore();
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("code-259", "title-602");
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
        MemoryPrimaryVendorProfileStoreFindsWhatItSaved();
    }
}
