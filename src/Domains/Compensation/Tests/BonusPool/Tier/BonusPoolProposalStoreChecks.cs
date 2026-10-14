namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolProposalStoreChecks
{
    public static void MemoryBonusPoolProposalStoreFindsWhatItSaved()
    {
        var store = BonusPoolProposalStore.NewMemoryBonusPoolProposalStore();
        var item = BonusPoolProposal.NewBonusPoolProposal("code-976", "title-549");
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
        MemoryBonusPoolProposalStoreFindsWhatItSaved();
    }
}
