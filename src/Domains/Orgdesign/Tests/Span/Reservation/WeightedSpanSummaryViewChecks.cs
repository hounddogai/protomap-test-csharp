namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedSpanSummaryViewChecks
{
    public static void WeightedSpanSummaryViewShowsTheStatusInCapitals()
    {
        var item = WeightedSpanSummary.NewWeightedSpanSummary("code-182", "title-261");
        var view = WeightedSpanSummaryView.ToWeightedSpanSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedSpanSummaryView.ToWeightedSpanSummaryViews(new List<WeightedSpanSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedSpanSummaryViewShowsTheStatusInCapitals();
    }
}
