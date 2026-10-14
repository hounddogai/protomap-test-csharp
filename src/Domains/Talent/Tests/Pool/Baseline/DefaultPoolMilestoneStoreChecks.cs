namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolMilestoneStoreChecks
{
    public static void MemoryDefaultPoolMilestoneStoreFindsWhatItSaved()
    {
        var store = DefaultPoolMilestoneStore.NewMemoryDefaultPoolMilestoneStore();
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("code-919", "name-870");
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
        MemoryDefaultPoolMilestoneStoreFindsWhatItSaved();
    }
}
