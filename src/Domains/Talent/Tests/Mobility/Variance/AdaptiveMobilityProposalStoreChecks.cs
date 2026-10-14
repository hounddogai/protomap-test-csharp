namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveMobilityProposalStoreChecks
{
    public static void MemoryAdaptiveMobilityProposalStoreFindsWhatItSaved()
    {
        var store = AdaptiveMobilityProposalStore.NewMemoryAdaptiveMobilityProposalStore();
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("code-202", "name-401");
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
        MemoryAdaptiveMobilityProposalStoreFindsWhatItSaved();
    }
}
