namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class RollingAccessBadgeProfileStoreChecks
{
    public static void MemoryRollingAccessBadgeProfileStoreFindsWhatItSaved()
    {
        var store = RollingAccessBadgeProfileStore.NewMemoryRollingAccessBadgeProfileStore();
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("code-54", "label-151");
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
        MemoryRollingAccessBadgeProfileStoreFindsWhatItSaved();
    }
}
