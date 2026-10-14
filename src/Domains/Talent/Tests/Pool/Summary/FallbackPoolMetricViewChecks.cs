namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackPoolMetricViewChecks
{
    public static void FallbackPoolMetricViewShowsTheStatusInCapitals()
    {
        var item = FallbackPoolMetric.NewFallbackPoolMetric("code-776", "name-246");
        var view = FallbackPoolMetricView.ToFallbackPoolMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPoolMetricView.ToFallbackPoolMetricViews(new List<FallbackPoolMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPoolMetricViewShowsTheStatusInCapitals();
    }
}
