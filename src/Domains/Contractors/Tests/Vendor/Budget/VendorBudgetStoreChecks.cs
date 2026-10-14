namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorBudgetStoreChecks
{
    public static void MemoryVendorBudgetStoreFindsWhatItSaved()
    {
        var store = VendorBudgetStore.NewMemoryVendorBudgetStore();
        var item = VendorBudget.NewVendorBudget("code-575", "name-607");
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
        MemoryVendorBudgetStoreFindsWhatItSaved();
    }
}
