namespace Acme.Workforce.Orgdesign.Tests.Span.Schedule;

using Acme.Workforce.Orgdesign.Span.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledSpanTierViewChecks
{
    public static void ScheduledSpanTierViewShowsTheStatusInCapitals()
    {
        var item = ScheduledSpanTier.NewScheduledSpanTier("code-491", "title-957");
        var view = ScheduledSpanTierView.ToScheduledSpanTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledSpanTierView.ToScheduledSpanTierViews(new List<ScheduledSpanTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledSpanTierViewShowsTheStatusInCapitals();
    }
}
