namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanScheduleStoreChecks
{
    public static void MemoryActionPlanScheduleStoreFindsWhatItSaved()
    {
        var store = ActionPlanScheduleStore.NewMemoryActionPlanScheduleStore();
        var item = ActionPlanSchedule.NewActionPlanSchedule("code-946", "name-141");
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
        MemoryActionPlanScheduleStoreFindsWhatItSaved();
    }
}
