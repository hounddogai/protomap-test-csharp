namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeAdjustmentStoreChecks
{
    public static void MemoryAccessBadgeAdjustmentStoreFindsWhatItSaved()
    {
        var store = AccessBadgeAdjustmentStore.NewMemoryAccessBadgeAdjustmentStore();
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("code-77", "title-805");
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
        MemoryAccessBadgeAdjustmentStoreFindsWhatItSaved();
    }
}
