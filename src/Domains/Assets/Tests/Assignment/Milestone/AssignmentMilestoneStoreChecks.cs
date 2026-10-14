namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentMilestoneStoreChecks
{
    public static void MemoryAssignmentMilestoneStoreFindsWhatItSaved()
    {
        var store = AssignmentMilestoneStore.NewMemoryAssignmentMilestoneStore();
        var item = AssignmentMilestone.NewAssignmentMilestone("code-989", "label-926");
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
        MemoryAssignmentMilestoneStoreFindsWhatItSaved();
    }
}
