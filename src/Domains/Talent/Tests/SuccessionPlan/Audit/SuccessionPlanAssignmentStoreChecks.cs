namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAssignmentStoreChecks
{
    public static void MemorySuccessionPlanAssignmentStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanAssignmentStore.NewMemorySuccessionPlanAssignmentStore();
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("code-428", "label-144");
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
        MemorySuccessionPlanAssignmentStoreFindsWhatItSaved();
    }
}
