namespace Acme.Workforce.Contractors.Tests.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkRecordViewChecks
{
    public static void LegacyStatementOfWorkRecordViewShowsTheStatusInCapitals()
    {
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("code-14", "name-98");
        var view = LegacyStatementOfWorkRecordView.ToLegacyStatementOfWorkRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyStatementOfWorkRecordView.ToLegacyStatementOfWorkRecordViews(new List<LegacyStatementOfWorkRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkRecordViewShowsTheStatusInCapitals();
    }
}
