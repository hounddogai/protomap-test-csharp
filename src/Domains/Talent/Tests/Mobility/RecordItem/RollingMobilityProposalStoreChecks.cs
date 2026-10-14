namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingMobilityProposalStoreChecks
{
    public static void MemoryRollingMobilityProposalStoreFindsWhatItSaved()
    {
        var store = RollingMobilityProposalStore.NewMemoryRollingMobilityProposalStore();
        var item = RollingMobilityProposal.NewRollingMobilityProposal("code-120", "label-142");
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
        MemoryRollingMobilityProposalStoreFindsWhatItSaved();
    }
}
