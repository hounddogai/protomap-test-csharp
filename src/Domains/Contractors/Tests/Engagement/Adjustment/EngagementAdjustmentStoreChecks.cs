namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementAdjustmentStoreChecks
{
    public static void MemoryEngagementAdjustmentStoreFindsWhatItSaved()
    {
        var store = EngagementAdjustmentStore.NewMemoryEngagementAdjustmentStore();
        var item = EngagementAdjustment.NewEngagementAdjustment("code-374", "title-852");
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
        MemoryEngagementAdjustmentStoreFindsWhatItSaved();
    }
}
