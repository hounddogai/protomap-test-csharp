namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionRecordViewChecks
{
    public static void PositionRecordViewShowsTheStatusInCapitals()
    {
        var item = PositionRecord.NewPositionRecord("code-490", "name-855");
        var view = PositionRecordView.ToPositionRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionRecordView.ToPositionRecordViews(new List<PositionRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionRecordViewShowsTheStatusInCapitals();
    }
}
