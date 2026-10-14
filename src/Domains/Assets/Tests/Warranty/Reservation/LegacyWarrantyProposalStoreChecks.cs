namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyWarrantyProposalStoreChecks
{
    public static void MemoryLegacyWarrantyProposalStoreFindsWhatItSaved()
    {
        var store = LegacyWarrantyProposalStore.NewMemoryLegacyWarrantyProposalStore();
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("code-988", "title-937");
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
        MemoryLegacyWarrantyProposalStoreFindsWhatItSaved();
    }
}
