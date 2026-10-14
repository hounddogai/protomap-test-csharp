namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveTownHallNoticeStoreChecks
{
    public static void MemoryAdaptiveTownHallNoticeStoreFindsWhatItSaved()
    {
        var store = AdaptiveTownHallNoticeStore.NewMemoryAdaptiveTownHallNoticeStore();
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("code-612", "title-436");
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
        MemoryAdaptiveTownHallNoticeStoreFindsWhatItSaved();
    }
}
