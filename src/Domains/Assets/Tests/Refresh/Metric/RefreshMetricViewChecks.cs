namespace Acme.Workforce.Assets.Tests.Refresh.Metric;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RefreshMetricViewChecks
{
    public static void RefreshMetricViewShowsTheStatusInCapitals()
    {
        var item = RefreshMetric.NewRefreshMetric("code-871", "name-310");
        var view = RefreshMetricView.ToRefreshMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshMetricView.ToRefreshMetricViews(new List<RefreshMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshMetricViewShowsTheStatusInCapitals();
    }
}
