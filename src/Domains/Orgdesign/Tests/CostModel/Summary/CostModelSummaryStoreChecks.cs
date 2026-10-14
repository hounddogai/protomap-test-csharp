namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CostModelSummaryStoreChecks
{
    public static void MemoryCostModelSummaryStoreFindsWhatItSaved()
    {
        var store = CostModelSummaryStore.NewMemoryCostModelSummaryStore();
        var item = CostModelSummary.NewCostModelSummary("code-497", "label-626");
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
        MemoryCostModelSummaryStoreFindsWhatItSaved();
    }
}
