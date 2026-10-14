namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictPoolMilestoneStoreChecks
{
    public static void MemoryStrictPoolMilestoneStoreFindsWhatItSaved()
    {
        var store = StrictPoolMilestoneStore.NewMemoryStrictPoolMilestoneStore();
        var item = StrictPoolMilestone.NewStrictPoolMilestone("code-516", "label-27");
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
        MemoryStrictPoolMilestoneStoreFindsWhatItSaved();
    }
}
