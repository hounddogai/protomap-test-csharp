namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccessBadgeClaimStoreChecks
{
    public static void MemoryAccessBadgeClaimStoreFindsWhatItSaved()
    {
        var store = AccessBadgeClaimStore.NewMemoryAccessBadgeClaimStore();
        var item = AccessBadgeClaim.NewAccessBadgeClaim("code-256", "name-962");
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
        MemoryAccessBadgeClaimStoreFindsWhatItSaved();
    }
}
