namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PendingReturnPlanRollupStoreChecks
{
    public static void MemoryPendingReturnPlanRollupStoreFindsWhatItSaved()
    {
        var store = PendingReturnPlanRollupStore.NewMemoryPendingReturnPlanRollupStore();
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("code-562", "title-355");
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
        MemoryPendingReturnPlanRollupStoreFindsWhatItSaved();
    }
}
