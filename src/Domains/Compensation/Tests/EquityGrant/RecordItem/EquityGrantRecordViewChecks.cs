namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantRecordViewChecks
{
    public static void EquityGrantRecordViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantRecord.NewEquityGrantRecord("code-424", "label-934");
        var view = EquityGrantRecordView.ToEquityGrantRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantRecordView.ToEquityGrantRecordViews(new List<EquityGrantRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantRecordViewShowsTheStatusInCapitals();
    }
}
