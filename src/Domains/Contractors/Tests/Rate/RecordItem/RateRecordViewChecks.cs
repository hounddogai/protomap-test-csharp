namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateRecordViewChecks
{
    public static void RateRecordViewShowsTheStatusInCapitals()
    {
        var item = RateRecord.NewRateRecord("code-196", "title-928");
        var view = RateRecordView.ToRateRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateRecordView.ToRateRecordViews(new List<RateRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateRecordViewShowsTheStatusInCapitals();
    }
}
