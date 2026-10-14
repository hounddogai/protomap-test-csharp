namespace Acme.Workforce.Engagement.Tests.TownHall.Claim;

using Acme.Workforce.Engagement.TownHall.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class TownHallQuotaStoreChecks
{
    public static void MemoryTownHallQuotaStoreFindsWhatItSaved()
    {
        var store = TownHallQuotaStore.NewMemoryTownHallQuotaStore();
        var item = TownHallQuota.NewTownHallQuota("code-628", "title-753");
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
        MemoryTownHallQuotaStoreFindsWhatItSaved();
    }
}
