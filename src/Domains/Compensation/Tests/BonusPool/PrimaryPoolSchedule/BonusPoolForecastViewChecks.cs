namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class BonusPoolForecastViewChecks
{
    public static void BonusPoolForecastViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolForecast.NewBonusPoolForecast("code-225", "title-62");
        var view = BonusPoolForecastView.ToBonusPoolForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolForecastView.ToBonusPoolForecastViews(new List<BonusPoolForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolForecastViewShowsTheStatusInCapitals();
    }
}
