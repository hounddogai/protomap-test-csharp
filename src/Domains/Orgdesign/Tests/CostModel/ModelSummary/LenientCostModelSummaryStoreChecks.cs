namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class LenientCostModelSummaryStoreChecks
{
    public static void MemoryLenientCostModelSummaryStoreFindsWhatItSaved()
    {
        var store = LenientCostModelSummaryStore.NewMemoryLenientCostModelSummaryStore();
        var item = LenientCostModelSummary.NewLenientCostModelSummary("code-309", "name-983");
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
        MemoryLenientCostModelSummaryStoreFindsWhatItSaved();
    }
}
