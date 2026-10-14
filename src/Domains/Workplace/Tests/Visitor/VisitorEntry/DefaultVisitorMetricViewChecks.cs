namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class DefaultVisitorMetricViewChecks
{
    public static void DefaultVisitorMetricViewShowsTheStatusInCapitals()
    {
        var item = DefaultVisitorMetric.NewDefaultVisitorMetric("code-277", "label-470");
        var view = DefaultVisitorMetricView.ToDefaultVisitorMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultVisitorMetricView.ToDefaultVisitorMetricViews(new List<DefaultVisitorMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultVisitorMetricViewShowsTheStatusInCapitals();
    }
}
