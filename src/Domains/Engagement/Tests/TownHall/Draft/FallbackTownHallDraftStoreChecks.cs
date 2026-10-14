namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackTownHallDraftStoreChecks
{
    public static void MemoryFallbackTownHallDraftStoreFindsWhatItSaved()
    {
        var store = FallbackTownHallDraftStore.NewMemoryFallbackTownHallDraftStore();
        var item = FallbackTownHallDraft.NewFallbackTownHallDraft("code-510", "label-59");
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
        MemoryFallbackTownHallDraftStoreFindsWhatItSaved();
    }
}
