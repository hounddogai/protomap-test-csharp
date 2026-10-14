namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorAssignmentStoreChecks
{
    public static void MemoryVendorAssignmentStoreFindsWhatItSaved()
    {
        var store = VendorAssignmentStore.NewMemoryVendorAssignmentStore();
        var item = VendorAssignment.NewVendorAssignment("code-126", "title-886");
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
        MemoryVendorAssignmentStoreFindsWhatItSaved();
    }
}
