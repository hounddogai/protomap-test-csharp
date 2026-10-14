namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingBalanceRecordViewChecks
{
    public static void RollingBalanceRecordViewShowsTheStatusInCapitals()
    {
        var item = RollingBalanceRecord.NewRollingBalanceRecord("code-730", "label-509");
        var view = RollingBalanceRecordView.ToRollingBalanceRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingBalanceRecordView.ToRollingBalanceRecordViews(new List<RollingBalanceRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingBalanceRecordViewShowsTheStatusInCapitals();
    }
}
