namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PolicySegmentStoreChecks
{
    public static void MemoryPolicySegmentStoreFindsWhatItSaved()
    {
        var store = PolicySegmentStore.NewMemoryPolicySegmentStore();
        var item = PolicySegment.NewPolicySegment("code-858", "name-967");
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
        MemoryPolicySegmentStoreFindsWhatItSaved();
    }
}
