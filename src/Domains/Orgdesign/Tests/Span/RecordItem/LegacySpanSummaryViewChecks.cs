namespace Acme.Workforce.Orgdesign.Tests.Span.RecordItem;

using Acme.Workforce.Orgdesign.Span.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacySpanSummaryViewChecks
{
    public static void LegacySpanSummaryViewShowsTheStatusInCapitals()
    {
        var item = LegacySpanSummary.NewLegacySpanSummary("code-200", "name-128");
        var view = LegacySpanSummaryView.ToLegacySpanSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacySpanSummaryView.ToLegacySpanSummaryViews(new List<LegacySpanSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacySpanSummaryViewShowsTheStatusInCapitals();
    }
}
