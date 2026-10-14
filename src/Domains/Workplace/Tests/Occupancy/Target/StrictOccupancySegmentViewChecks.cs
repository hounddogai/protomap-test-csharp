namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictOccupancySegmentViewChecks
{
    public static void StrictOccupancySegmentViewShowsTheStatusInCapitals()
    {
        var item = StrictOccupancySegment.NewStrictOccupancySegment("code-95", "label-956");
        var view = StrictOccupancySegmentView.ToStrictOccupancySegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictOccupancySegmentView.ToStrictOccupancySegmentViews(new List<StrictOccupancySegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictOccupancySegmentViewShowsTheStatusInCapitals();
    }
}
