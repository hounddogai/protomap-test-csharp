namespace Acme.Workforce.Compensation.Tests.Adjustment.Estimate;

using Acme.Workforce.Compensation.Adjustment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalAdjustmentProposalStoreChecks
{
    public static void MemorySeasonalAdjustmentProposalStoreFindsWhatItSaved()
    {
        var store = SeasonalAdjustmentProposalStore.NewMemorySeasonalAdjustmentProposalStore();
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("code-769", "label-55");
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
        MemorySeasonalAdjustmentProposalStoreFindsWhatItSaved();
    }
}
