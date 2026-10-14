namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class PendingOccupancyNoticeChecks
{
    public static void PendingOccupancyNoticeStartsInRequested()
    {
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("code-659", "name-670");
        Check.Equal(item.Status, PendingOccupancyNoticeStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingOccupancyNoticeMovesToRevoked()
    {
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("code-659", "name-670");
        try
        {
            item.MoveTo(PendingOccupancyNoticeStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingOccupancyNoticeStatus.Revoked, "status");
    }

    public static void PendingOccupancyNoticeRejectsMovesFromExpired()
    {
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("code-659", "name-670");
        item.Status = PendingOccupancyNoticeStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingOccupancyNoticeStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingOccupancyNoticeStartsInRequested();
        PendingOccupancyNoticeMovesToRevoked();
        PendingOccupancyNoticeRejectsMovesFromExpired();
    }
}
