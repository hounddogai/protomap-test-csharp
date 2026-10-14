namespace Acme.Workforce.Engagement.Tests.TownHall.Window;

using Acme.Workforce.Engagement.TownHall.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TownHallWindowStoreChecks
{
    public static void MemoryTownHallWindowStoreFindsWhatItSaved()
    {
        var store = TownHallWindowStore.NewMemoryTownHallWindowStore();
        var item = TownHallWindow.NewTownHallWindow("code-54", "title-666");
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
        MemoryTownHallWindowStoreFindsWhatItSaved();
    }
}
