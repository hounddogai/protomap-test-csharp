namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class WeightedAdjustmentTemplateStoreChecks
{
    public static void MemoryWeightedAdjustmentTemplateStoreFindsWhatItSaved()
    {
        var store = WeightedAdjustmentTemplateStore.NewMemoryWeightedAdjustmentTemplateStore();
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("code-4", "label-838");
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
        MemoryWeightedAdjustmentTemplateStoreFindsWhatItSaved();
    }
}
