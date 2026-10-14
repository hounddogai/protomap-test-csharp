namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkVarianceViewChecks
{
    public static void ScheduledStatementOfWorkVarianceViewShowsTheStatusInCapitals()
    {
        var item = ScheduledStatementOfWorkVariance.NewScheduledStatementOfWorkVariance("code-868", "name-629");
        var view = ScheduledStatementOfWorkVarianceView.ToScheduledStatementOfWorkVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledStatementOfWorkVarianceView.ToScheduledStatementOfWorkVarianceViews(new List<ScheduledStatementOfWorkVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkVarianceViewShowsTheStatusInCapitals();
    }
}
