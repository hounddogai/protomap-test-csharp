namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LenientRefreshProfileStoreChecks
{
    public static void MemoryLenientRefreshProfileStoreFindsWhatItSaved()
    {
        var store = LenientRefreshProfileStore.NewMemoryLenientRefreshProfileStore();
        var item = LenientRefreshProfile.NewLenientRefreshProfile("code-464", "label-581");
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
        MemoryLenientRefreshProfileStoreFindsWhatItSaved();
    }
}
