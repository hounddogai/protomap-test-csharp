namespace Acme.Workforce.Talent.Tests.Pool.Assignment;

using Acme.Workforce.Talent.Pool.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AdaptivePoolAssignmentStoreChecks
{
    public static void MemoryAdaptivePoolAssignmentStoreFindsWhatItSaved()
    {
        var store = AdaptivePoolAssignmentStore.NewMemoryAdaptivePoolAssignmentStore();
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("code-311", "label-333");
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
        MemoryAdaptivePoolAssignmentStoreFindsWhatItSaved();
    }
}
