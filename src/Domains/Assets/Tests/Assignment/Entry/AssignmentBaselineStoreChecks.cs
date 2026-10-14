namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentBaselineStoreChecks
{
    public static void MemoryAssignmentBaselineStoreFindsWhatItSaved()
    {
        var store = AssignmentBaselineStore.NewMemoryAssignmentBaselineStore();
        var item = AssignmentBaseline.NewAssignmentBaseline("code-669", "label-116");
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
        MemoryAssignmentBaselineStoreFindsWhatItSaved();
    }
}
