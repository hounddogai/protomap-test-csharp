namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanScheduleViewChecks
{
    public static void SpanScheduleViewShowsTheStatusInCapitals()
    {
        var item = SpanSchedule.NewSpanSchedule("code-160", "name-882");
        var view = SpanScheduleView.ToSpanScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanScheduleView.ToSpanScheduleViews(new List<SpanSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanScheduleViewShowsTheStatusInCapitals();
    }
}
