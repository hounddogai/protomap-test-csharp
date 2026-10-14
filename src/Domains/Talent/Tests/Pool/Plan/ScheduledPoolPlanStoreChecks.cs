namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledPoolPlanStoreChecks
{
    public static void MemoryScheduledPoolPlanStoreFindsWhatItSaved()
    {
        var store = ScheduledPoolPlanStore.NewMemoryScheduledPoolPlanStore();
        var item = ScheduledPoolPlan.NewScheduledPoolPlan("code-541", "name-377");
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
        MemoryScheduledPoolPlanStoreFindsWhatItSaved();
    }
}
