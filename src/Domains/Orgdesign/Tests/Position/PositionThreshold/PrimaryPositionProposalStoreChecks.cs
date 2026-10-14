namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PrimaryPositionProposalStoreChecks
{
    public static void MemoryPrimaryPositionProposalStoreFindsWhatItSaved()
    {
        var store = PrimaryPositionProposalStore.NewMemoryPrimaryPositionProposalStore();
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("code-819", "label-613");
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
        MemoryPrimaryPositionProposalStoreFindsWhatItSaved();
    }
}
