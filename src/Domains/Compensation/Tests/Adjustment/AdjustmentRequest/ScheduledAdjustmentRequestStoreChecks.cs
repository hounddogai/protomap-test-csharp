namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class ScheduledAdjustmentRequestStoreChecks
{
    public static void MemoryScheduledAdjustmentRequestStoreFindsWhatItSaved()
    {
        var store = ScheduledAdjustmentRequestStore.NewMemoryScheduledAdjustmentRequestStore();
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("code-312", "label-378");
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
        MemoryScheduledAdjustmentRequestStoreFindsWhatItSaved();
    }
}
