namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class RollingAccessBadgeRequestStoreChecks
{
    public static void MemoryRollingAccessBadgeRequestStoreFindsWhatItSaved()
    {
        var store = RollingAccessBadgeRequestStore.NewMemoryRollingAccessBadgeRequestStore();
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("code-645", "label-901");
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
        MemoryRollingAccessBadgeRequestStoreFindsWhatItSaved();
    }
}
