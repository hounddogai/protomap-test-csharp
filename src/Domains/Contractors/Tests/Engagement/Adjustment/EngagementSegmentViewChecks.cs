namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementSegmentViewChecks
{
    public static void EngagementSegmentViewShowsTheStatusInCapitals()
    {
        var item = EngagementSegment.NewEngagementSegment("code-241", "title-193");
        var view = EngagementSegmentView.ToEngagementSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementSegmentView.ToEngagementSegmentViews(new List<EngagementSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementSegmentViewShowsTheStatusInCapitals();
    }
}
