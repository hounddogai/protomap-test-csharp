namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingTownHallThresholdStoreChecks
{
    public static void MemoryRollingTownHallThresholdStoreFindsWhatItSaved()
    {
        var store = RollingTownHallThresholdStore.NewMemoryRollingTownHallThresholdStore();
        var item = RollingTownHallThreshold.NewRollingTownHallThreshold("code-441", "title-155");
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
        MemoryRollingTownHallThresholdStoreFindsWhatItSaved();
    }
}
