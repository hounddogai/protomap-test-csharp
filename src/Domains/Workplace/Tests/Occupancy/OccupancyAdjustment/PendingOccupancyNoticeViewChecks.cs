namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class PendingOccupancyNoticeViewChecks
{
    public static void PendingOccupancyNoticeViewShowsTheStatusInCapitals()
    {
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("code-659", "name-670");
        var view = PendingOccupancyNoticeView.ToPendingOccupancyNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingOccupancyNoticeView.ToPendingOccupancyNoticeViews(new List<PendingOccupancyNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingOccupancyNoticeViewShowsTheStatusInCapitals();
    }
}
