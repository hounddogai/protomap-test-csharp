namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackAdjustmentProposalStoreChecks
{
    public static void MemoryFallbackAdjustmentProposalStoreFindsWhatItSaved()
    {
        var store = FallbackAdjustmentProposalStore.NewMemoryFallbackAdjustmentProposalStore();
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("code-487", "label-265");
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
        MemoryFallbackAdjustmentProposalStoreFindsWhatItSaved();
    }
}
