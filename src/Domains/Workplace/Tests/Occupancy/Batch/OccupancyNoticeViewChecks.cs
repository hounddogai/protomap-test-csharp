namespace Acme.Workforce.Workplace.Tests.Occupancy.Batch;

using Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class OccupancyNoticeViewChecks
{
    public static void OccupancyNoticeViewShowsTheStatusInCapitals()
    {
        var item = OccupancyNotice.NewOccupancyNotice("code-272", "name-358");
        var view = OccupancyNoticeView.ToOccupancyNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyNoticeView.ToOccupancyNoticeViews(new List<OccupancyNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyNoticeViewShowsTheStatusInCapitals();
    }
}
