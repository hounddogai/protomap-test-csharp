namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultAccessBadgeOverrideStoreChecks
{
    public static void MemoryDefaultAccessBadgeOverrideStoreFindsWhatItSaved()
    {
        var store = DefaultAccessBadgeOverrideStore.NewMemoryDefaultAccessBadgeOverrideStore();
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("code-529", "title-767");
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
        MemoryDefaultAccessBadgeOverrideStoreFindsWhatItSaved();
    }
}
