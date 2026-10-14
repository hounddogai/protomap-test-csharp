namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Vendor.VendorBudget;

/// <summary>Checks of vendor budget code, run by the project's runner.</summary>
public static class TieredVendorBudgetStoreChecks
{
    public static void MemoryTieredVendorBudgetStoreFindsWhatItSaved()
    {
        var store = TieredVendorBudgetStore.NewMemoryTieredVendorBudgetStore();
        var item = TieredVendorBudget.NewTieredVendorBudget("code-571", "label-228");
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
        MemoryTieredVendorBudgetStoreFindsWhatItSaved();
    }
}
