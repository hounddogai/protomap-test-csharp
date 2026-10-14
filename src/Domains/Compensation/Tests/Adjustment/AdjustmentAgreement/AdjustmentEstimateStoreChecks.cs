namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class AdjustmentEstimateStoreChecks
{
    public static void MemoryAdjustmentEstimateStoreFindsWhatItSaved()
    {
        var store = AdjustmentEstimateStore.NewMemoryAdjustmentEstimateStore();
        var item = AdjustmentEstimate.NewAdjustmentEstimate("code-726", "title-383");
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
        MemoryAdjustmentEstimateStoreFindsWhatItSaved();
    }
}
