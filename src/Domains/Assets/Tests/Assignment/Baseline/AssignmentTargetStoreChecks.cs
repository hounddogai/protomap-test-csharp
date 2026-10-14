namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentTargetStoreChecks
{
    public static void MemoryAssignmentTargetStoreFindsWhatItSaved()
    {
        var store = AssignmentTargetStore.NewMemoryAssignmentTargetStore();
        var item = AssignmentTarget.NewAssignmentTarget("code-575", "label-783");
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
        MemoryAssignmentTargetStoreFindsWhatItSaved();
    }
}
