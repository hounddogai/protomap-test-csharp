namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class RegionalLicenseSeatMilestoneStoreChecks
{
    public static void MemoryRegionalLicenseSeatMilestoneStoreFindsWhatItSaved()
    {
        var store = RegionalLicenseSeatMilestoneStore.NewMemoryRegionalLicenseSeatMilestoneStore();
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("code-50", "title-951");
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
        MemoryRegionalLicenseSeatMilestoneStoreFindsWhatItSaved();
    }
}
