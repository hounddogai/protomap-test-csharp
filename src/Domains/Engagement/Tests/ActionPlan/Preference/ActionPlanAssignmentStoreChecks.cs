namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanAssignmentStoreChecks
{
    public static void MemoryActionPlanAssignmentStoreFindsWhatItSaved()
    {
        var store = ActionPlanAssignmentStore.NewMemoryActionPlanAssignmentStore();
        var item = ActionPlanAssignment.NewActionPlanAssignment("code-73", "title-697");
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
        MemoryActionPlanAssignmentStoreFindsWhatItSaved();
    }
}
