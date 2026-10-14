namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TieredPoolMetricViewChecks
{
    public static void TieredPoolMetricViewShowsTheStatusInCapitals()
    {
        var item = TieredPoolMetric.NewTieredPoolMetric("code-683", "label-94");
        var view = TieredPoolMetricView.ToTieredPoolMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPoolMetricView.ToTieredPoolMetricViews(new List<TieredPoolMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPoolMetricViewShowsTheStatusInCapitals();
    }
}
