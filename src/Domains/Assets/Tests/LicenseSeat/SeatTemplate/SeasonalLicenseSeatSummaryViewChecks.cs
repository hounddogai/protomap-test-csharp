namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatSummaryViewChecks
{
    public static void SeasonalLicenseSeatSummaryViewShowsTheStatusInCapitals()
    {
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("code-649", "title-932");
        var view = SeasonalLicenseSeatSummaryView.ToSeasonalLicenseSeatSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalLicenseSeatSummaryView.ToSeasonalLicenseSeatSummaryViews(new List<SeasonalLicenseSeatSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatSummaryViewShowsTheStatusInCapitals();
    }
}
