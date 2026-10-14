namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class AdjustmentTargetStoreChecks
{
    public static void MemoryAdjustmentTargetStoreFindsWhatItSaved()
    {
        var store = AdjustmentTargetStore.NewMemoryAdjustmentTargetStore();
        var item = AdjustmentTarget.NewAdjustmentTarget("code-12", "label-137");
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
        MemoryAdjustmentTargetStoreFindsWhatItSaved();
    }
}
