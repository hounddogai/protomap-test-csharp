namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class RollingVendorQuotaStoreChecks
{
    public static void MemoryRollingVendorQuotaStoreFindsWhatItSaved()
    {
        var store = RollingVendorQuotaStore.NewMemoryRollingVendorQuotaStore();
        var item = RollingVendorQuota.NewRollingVendorQuota("code-268", "label-803");
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
        MemoryRollingVendorQuotaStoreFindsWhatItSaved();
    }
}
