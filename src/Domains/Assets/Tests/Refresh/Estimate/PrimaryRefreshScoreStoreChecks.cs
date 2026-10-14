namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshScoreStoreChecks
{
    public static void MemoryPrimaryRefreshScoreStoreFindsWhatItSaved()
    {
        var store = PrimaryRefreshScoreStore.NewMemoryPrimaryRefreshScoreStore();
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("code-801", "title-654");
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
        MemoryPrimaryRefreshScoreStoreFindsWhatItSaved();
    }
}
