namespace Acme.Workforce.Engagement.Tests.TownHall.OverrideItem;

using Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TownHallOverrideStoreChecks
{
    public static void MemoryTownHallOverrideStoreFindsWhatItSaved()
    {
        var store = TownHallOverrideStore.NewMemoryTownHallOverrideStore();
        var item = TownHallOverride.NewTownHallOverride("code-520", "title-865");
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
        MemoryTownHallOverrideStoreFindsWhatItSaved();
    }
}
