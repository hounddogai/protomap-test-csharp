namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkSegmentViewChecks
{
    public static void ScheduledStatementOfWorkSegmentViewShowsTheStatusInCapitals()
    {
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("code-861", "name-956");
        var view = ScheduledStatementOfWorkSegmentView.ToScheduledStatementOfWorkSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledStatementOfWorkSegmentView.ToScheduledStatementOfWorkSegmentViews(new List<ScheduledStatementOfWorkSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkSegmentViewShowsTheStatusInCapitals();
    }
}
