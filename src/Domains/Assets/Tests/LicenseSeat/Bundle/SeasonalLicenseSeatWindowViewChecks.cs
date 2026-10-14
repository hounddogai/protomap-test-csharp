namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatWindowViewChecks
{
    public static void SeasonalLicenseSeatWindowViewShowsTheStatusInCapitals()
    {
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("code-626", "title-437");
        var view = SeasonalLicenseSeatWindowView.ToSeasonalLicenseSeatWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalLicenseSeatWindowView.ToSeasonalLicenseSeatWindowViews(new List<SeasonalLicenseSeatWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatWindowViewShowsTheStatusInCapitals();
    }
}
