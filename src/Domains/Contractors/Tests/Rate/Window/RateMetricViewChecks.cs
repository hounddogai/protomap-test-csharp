namespace Acme.Workforce.Contractors.Tests.Rate.Window;

using Acme.Workforce.Contractors.Rate.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RateMetricViewChecks
{
    public static void RateMetricViewShowsTheStatusInCapitals()
    {
        var item = RateMetric.NewRateMetric("code-341", "name-658");
        var view = RateMetricView.ToRateMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateMetricView.ToRateMetricViews(new List<RateMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateMetricViewShowsTheStatusInCapitals();
    }
}
