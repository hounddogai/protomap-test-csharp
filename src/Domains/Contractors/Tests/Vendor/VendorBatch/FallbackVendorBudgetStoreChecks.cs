namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBatch;

using Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>Checks of vendor batch code, run by the project's runner.</summary>
public static class FallbackVendorBudgetStoreChecks
{
    public static void MemoryFallbackVendorBudgetStoreFindsWhatItSaved()
    {
        var store = FallbackVendorBudgetStore.NewMemoryFallbackVendorBudgetStore();
        var item = FallbackVendorBudget.NewFallbackVendorBudget("code-306", "title-985");
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
        MemoryFallbackVendorBudgetStoreFindsWhatItSaved();
    }
}
