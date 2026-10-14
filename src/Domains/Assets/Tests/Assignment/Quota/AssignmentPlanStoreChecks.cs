namespace Acme.Workforce.Assets.Tests.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssignmentPlanStoreChecks
{
    public static void MemoryAssignmentPlanStoreFindsWhatItSaved()
    {
        var store = AssignmentPlanStore.NewMemoryAssignmentPlanStore();
        var item = AssignmentPlan.NewAssignmentPlan("code-89", "name-693");
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
        MemoryAssignmentPlanStoreFindsWhatItSaved();
    }
}
