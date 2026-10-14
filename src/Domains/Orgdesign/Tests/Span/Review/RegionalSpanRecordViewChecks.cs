namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalSpanRecordViewChecks
{
    public static void RegionalSpanRecordViewShowsTheStatusInCapitals()
    {
        var item = RegionalSpanRecord.NewRegionalSpanRecord("code-687", "title-403");
        var view = RegionalSpanRecordView.ToRegionalSpanRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalSpanRecordView.ToRegionalSpanRecordViews(new List<RegionalSpanRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalSpanRecordViewShowsTheStatusInCapitals();
    }
}
