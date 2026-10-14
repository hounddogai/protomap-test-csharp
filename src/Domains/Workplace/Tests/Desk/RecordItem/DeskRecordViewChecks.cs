namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskRecordViewChecks
{
    public static void DeskRecordViewShowsTheStatusInCapitals()
    {
        var item = DeskRecord.NewDeskRecord("code-781", "title-197");
        var view = DeskRecordView.ToDeskRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskRecordView.ToDeskRecordViews(new List<DeskRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskRecordViewShowsTheStatusInCapitals();
    }
}
