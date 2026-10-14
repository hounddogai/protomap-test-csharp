namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkRecordViewChecks
{
    public static void StatementOfWorkRecordViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkRecord.NewStatementOfWorkRecord("code-4", "name-63");
        var view = StatementOfWorkRecordView.ToStatementOfWorkRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkRecordView.ToStatementOfWorkRecordViews(new List<StatementOfWorkRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkRecordViewShowsTheStatusInCapitals();
    }
}
