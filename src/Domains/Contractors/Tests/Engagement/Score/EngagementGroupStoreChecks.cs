namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class EngagementGroupStoreChecks
{
    public static void MemoryEngagementGroupStoreFindsWhatItSaved()
    {
        var store = EngagementGroupStore.NewMemoryEngagementGroupStore();
        var item = EngagementGroup.NewEngagementGroup("code-572", "label-646");
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
        MemoryEngagementGroupStoreFindsWhatItSaved();
    }
}
