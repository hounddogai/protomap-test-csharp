namespace Acme.Workforce.Workplace.Tests.Occupancy.Batch;

using Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class OccupancyNoticeChecks
{
    public static void OccupancyNoticeStartsInOpen()
    {
        var item = OccupancyNotice.NewOccupancyNotice("code-272", "name-358");
        Check.Equal(item.Status, OccupancyNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyNoticeMovesToArchived()
    {
        var item = OccupancyNotice.NewOccupancyNotice("code-272", "name-358");
        try
        {
            item.MoveTo(OccupancyNoticeStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyNoticeStatus.Archived, "status");
    }

    public static void OccupancyNoticeRejectsMovesFromArchived()
    {
        var item = OccupancyNotice.NewOccupancyNotice("code-272", "name-358");
        item.Status = OccupancyNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(OccupancyNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        OccupancyNoticeStartsInOpen();
        OccupancyNoticeMovesToArchived();
        OccupancyNoticeRejectsMovesFromArchived();
    }
}
