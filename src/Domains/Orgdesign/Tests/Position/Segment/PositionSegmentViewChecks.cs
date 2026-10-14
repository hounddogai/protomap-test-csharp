namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PositionSegmentViewChecks
{
    public static void PositionSegmentViewShowsTheStatusInCapitals()
    {
        var item = PositionSegment.NewPositionSegment("code-563", "label-709");
        var view = PositionSegmentView.ToPositionSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionSegmentView.ToPositionSegmentViews(new List<PositionSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionSegmentViewShowsTheStatusInCapitals();
    }
}
