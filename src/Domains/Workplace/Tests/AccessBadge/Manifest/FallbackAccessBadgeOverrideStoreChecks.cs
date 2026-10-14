namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackAccessBadgeOverrideStoreChecks
{
    public static void MemoryFallbackAccessBadgeOverrideStoreFindsWhatItSaved()
    {
        var store = FallbackAccessBadgeOverrideStore.NewMemoryFallbackAccessBadgeOverrideStore();
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("code-278", "title-640");
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
        MemoryFallbackAccessBadgeOverrideStoreFindsWhatItSaved();
    }
}
