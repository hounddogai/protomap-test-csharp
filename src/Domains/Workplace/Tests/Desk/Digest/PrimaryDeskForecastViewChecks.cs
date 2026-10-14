namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PrimaryDeskForecastViewChecks
{
    public static void PrimaryDeskForecastViewShowsTheStatusInCapitals()
    {
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("code-737", "label-320");
        var view = PrimaryDeskForecastView.ToPrimaryDeskForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryDeskForecastView.ToPrimaryDeskForecastViews(new List<PrimaryDeskForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryDeskForecastViewShowsTheStatusInCapitals();
    }
}
