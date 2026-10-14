namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyProposalStoreChecks
{
    public static void MemoryWarrantyProposalStoreFindsWhatItSaved()
    {
        var store = WarrantyProposalStore.NewMemoryWarrantyProposalStore();
        var item = WarrantyProposal.NewWarrantyProposal("code-331", "title-473");
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
        MemoryWarrantyProposalStoreFindsWhatItSaved();
    }
}
