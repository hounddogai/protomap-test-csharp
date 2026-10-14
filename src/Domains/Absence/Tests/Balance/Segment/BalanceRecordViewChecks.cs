namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceRecordViewChecks
{
    public static void BalanceRecordViewShowsTheStatusInCapitals()
    {
        var item = BalanceRecord.NewBalanceRecord("code-341", "name-114");
        var view = BalanceRecordView.ToBalanceRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceRecordView.ToBalanceRecordViews(new List<BalanceRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceRecordViewShowsTheStatusInCapitals();
    }
}
