namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshScoreStoreChecks
{
    public static void MemoryRefreshScoreStoreFindsWhatItSaved()
    {
        var store = RefreshScoreStore.NewMemoryRefreshScoreStore();
        var item = RefreshScore.NewRefreshScore("code-57", "name-499");
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
        MemoryRefreshScoreStoreFindsWhatItSaved();
    }
}
