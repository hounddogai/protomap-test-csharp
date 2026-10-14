namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SeasonalRefreshWindowStoreChecks
{
    public static void MemorySeasonalRefreshWindowStoreFindsWhatItSaved()
    {
        var store = SeasonalRefreshWindowStore.NewMemorySeasonalRefreshWindowStore();
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("code-555", "title-218");
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
        MemorySeasonalRefreshWindowStoreFindsWhatItSaved();
    }
}
