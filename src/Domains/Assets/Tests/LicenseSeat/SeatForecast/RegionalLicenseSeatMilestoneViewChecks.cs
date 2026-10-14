namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class RegionalLicenseSeatMilestoneViewChecks
{
    public static void RegionalLicenseSeatMilestoneViewShowsTheStatusInCapitals()
    {
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("code-50", "title-951");
        var view = RegionalLicenseSeatMilestoneView.ToRegionalLicenseSeatMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalLicenseSeatMilestoneView.ToRegionalLicenseSeatMilestoneViews(new List<RegionalLicenseSeatMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalLicenseSeatMilestoneViewShowsTheStatusInCapitals();
    }
}
