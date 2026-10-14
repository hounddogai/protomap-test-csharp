namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMetricViewChecks
{
    public static void AccessBadgeMetricViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeMetric.NewAccessBadgeMetric("code-173", "label-849");
        var view = AccessBadgeMetricView.ToAccessBadgeMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeMetricView.ToAccessBadgeMetricViews(new List<AccessBadgeMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeMetricViewShowsTheStatusInCapitals();
    }
}
