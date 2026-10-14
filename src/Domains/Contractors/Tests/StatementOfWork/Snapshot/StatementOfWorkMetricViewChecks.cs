namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkMetricViewChecks
{
    public static void StatementOfWorkMetricViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkMetric.NewStatementOfWorkMetric("code-737", "title-546");
        var view = StatementOfWorkMetricView.ToStatementOfWorkMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkMetricView.ToStatementOfWorkMetricViews(new List<StatementOfWorkMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkMetricViewShowsTheStatusInCapitals();
    }
}
