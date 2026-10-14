namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedBalanceSummaryStoreChecks
{
    public static void MemoryWeightedBalanceSummaryStoreFindsWhatItSaved()
    {
        var store = WeightedBalanceSummaryStore.NewMemoryWeightedBalanceSummaryStore();
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("code-590", "label-598");
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
        MemoryWeightedBalanceSummaryStoreFindsWhatItSaved();
    }
}
