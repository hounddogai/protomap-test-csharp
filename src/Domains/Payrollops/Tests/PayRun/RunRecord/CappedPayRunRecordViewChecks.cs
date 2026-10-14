namespace Acme.Workforce.Payrollops.Tests.PayRun.RunRecord;

using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Checks of run record code, run by the project's runner.</summary>
public static class CappedPayRunRecordViewChecks
{
    public static void CappedPayRunRecordViewShowsTheStatusInCapitals()
    {
        var item = CappedPayRunRecord.NewCappedPayRunRecord("code-986", "name-43");
        var view = CappedPayRunRecordView.ToCappedPayRunRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPayRunRecordView.ToCappedPayRunRecordViews(new List<CappedPayRunRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPayRunRecordViewShowsTheStatusInCapitals();
    }
}
