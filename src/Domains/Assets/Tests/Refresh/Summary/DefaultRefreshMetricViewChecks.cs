namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultRefreshMetricViewChecks
{
    public static void DefaultRefreshMetricViewShowsTheStatusInCapitals()
    {
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("code-607", "name-959");
        var view = DefaultRefreshMetricView.ToDefaultRefreshMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultRefreshMetricView.ToDefaultRefreshMetricViews(new List<DefaultRefreshMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultRefreshMetricViewShowsTheStatusInCapitals();
    }
}
