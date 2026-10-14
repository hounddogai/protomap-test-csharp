namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskMetricViewChecks
{
    public static void DeskMetricViewShowsTheStatusInCapitals()
    {
        var item = DeskMetric.NewDeskMetric("code-31", "name-846");
        var view = DeskMetricView.ToDeskMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskMetricView.ToDeskMetricViews(new List<DeskMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskMetricViewShowsTheStatusInCapitals();
    }
}
