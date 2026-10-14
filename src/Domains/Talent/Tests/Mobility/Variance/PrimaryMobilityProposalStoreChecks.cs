namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PrimaryMobilityProposalStoreChecks
{
    public static void MemoryPrimaryMobilityProposalStoreFindsWhatItSaved()
    {
        var store = PrimaryMobilityProposalStore.NewMemoryPrimaryMobilityProposalStore();
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("code-902", "name-684");
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
        MemoryPrimaryMobilityProposalStoreFindsWhatItSaved();
    }
}
