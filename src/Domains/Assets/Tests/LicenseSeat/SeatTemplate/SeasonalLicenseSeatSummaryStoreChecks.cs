namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatSummaryStoreChecks
{
    public static void MemorySeasonalLicenseSeatSummaryStoreFindsWhatItSaved()
    {
        var store = SeasonalLicenseSeatSummaryStore.NewMemorySeasonalLicenseSeatSummaryStore();
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("code-649", "title-932");
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
        MemorySeasonalLicenseSeatSummaryStoreFindsWhatItSaved();
    }
}
