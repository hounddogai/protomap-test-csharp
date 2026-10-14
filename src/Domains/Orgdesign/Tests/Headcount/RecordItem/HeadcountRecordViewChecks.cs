namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountRecordViewChecks
{
    public static void HeadcountRecordViewShowsTheStatusInCapitals()
    {
        var item = HeadcountRecord.NewHeadcountRecord("code-849", "name-289");
        var view = HeadcountRecordView.ToHeadcountRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountRecordView.ToHeadcountRecordViews(new List<HeadcountRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountRecordViewShowsTheStatusInCapitals();
    }
}
