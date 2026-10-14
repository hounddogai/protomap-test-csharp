namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class SeasonalPayBandCycleViewChecks
{
    public static void SeasonalPayBandCycleViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("code-524", "label-3");
        var view = SeasonalPayBandCycleView.ToSeasonalPayBandCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPayBandCycleView.ToSeasonalPayBandCycleViews(new List<SeasonalPayBandCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPayBandCycleViewShowsTheStatusInCapitals();
    }
}
