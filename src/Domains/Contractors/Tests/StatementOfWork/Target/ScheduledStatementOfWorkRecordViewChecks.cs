namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkRecordViewChecks
{
    public static void ScheduledStatementOfWorkRecordViewShowsTheStatusInCapitals()
    {
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("code-240", "name-371");
        var view = ScheduledStatementOfWorkRecordView.ToScheduledStatementOfWorkRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledStatementOfWorkRecordView.ToScheduledStatementOfWorkRecordViews(new List<ScheduledStatementOfWorkRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkRecordViewShowsTheStatusInCapitals();
    }
}
