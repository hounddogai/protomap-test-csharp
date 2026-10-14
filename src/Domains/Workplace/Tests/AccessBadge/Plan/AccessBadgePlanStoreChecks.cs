namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AccessBadgePlanStoreChecks
{
    public static void MemoryAccessBadgePlanStoreFindsWhatItSaved()
    {
        var store = AccessBadgePlanStore.NewMemoryAccessBadgePlanStore();
        var item = AccessBadgePlan.NewAccessBadgePlan("code-720", "name-277");
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
        MemoryAccessBadgePlanStoreFindsWhatItSaved();
    }
}
