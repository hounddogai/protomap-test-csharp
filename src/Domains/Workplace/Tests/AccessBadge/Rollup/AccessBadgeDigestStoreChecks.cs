namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeDigestStoreChecks
{
    public static void MemoryAccessBadgeDigestStoreFindsWhatItSaved()
    {
        var store = AccessBadgeDigestStore.NewMemoryAccessBadgeDigestStore();
        var item = AccessBadgeDigest.NewAccessBadgeDigest("code-101", "name-800");
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
        MemoryAccessBadgeDigestStoreFindsWhatItSaved();
    }
}
