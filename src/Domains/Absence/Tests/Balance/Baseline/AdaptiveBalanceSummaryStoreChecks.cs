namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceSummaryStoreChecks
{
    public static void MemoryAdaptiveBalanceSummaryStoreFindsWhatItSaved()
    {
        var store = AdaptiveBalanceSummaryStore.NewMemoryAdaptiveBalanceSummaryStore();
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("code-456", "label-781");
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
        MemoryAdaptiveBalanceSummaryStoreFindsWhatItSaved();
    }
}
