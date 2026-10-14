namespace Acme.Workforce.Engagement.Tests.TownHall.HallRule;

using Acme.Workforce.Engagement.TownHall.HallRule;

/// <summary>Checks of hall rule code, run by the project's runner.</summary>
public static class TownHallDraftStoreChecks
{
    public static void MemoryTownHallDraftStoreFindsWhatItSaved()
    {
        var store = TownHallDraftStore.NewMemoryTownHallDraftStore();
        var item = TownHallDraft.NewTownHallDraft("code-59", "name-464");
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
        MemoryTownHallDraftStoreFindsWhatItSaved();
    }
}
