namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentEstimateStoreChecks
{
    public static void MemoryAssignmentEstimateStoreFindsWhatItSaved()
    {
        var store = AssignmentEstimateStore.NewMemoryAssignmentEstimateStore();
        var item = AssignmentEstimate.NewAssignmentEstimate("code-809", "name-62");
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
        MemoryAssignmentEstimateStoreFindsWhatItSaved();
    }
}
