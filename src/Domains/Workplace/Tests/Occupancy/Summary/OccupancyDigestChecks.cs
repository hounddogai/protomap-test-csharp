namespace Acme.Workforce.Workplace.Tests.Occupancy.Summary;

using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class OccupancyDigestChecks
{
    public static void OccupancyDigestStartsInIdle()
    {
        var item = OccupancyDigest.NewOccupancyDigest("code-743", "label-365");
        Check.Equal(item.Status, OccupancyDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyDigestMovesToReady()
    {
        var item = OccupancyDigest.NewOccupancyDigest("code-743", "label-365");
        try
        {
            item.MoveTo(OccupancyDigestStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyDigestStatus.Ready, "status");
    }

    public static void OccupancyDigestRejectsMovesFromStopped()
    {
        var item = OccupancyDigest.NewOccupancyDigest("code-743", "label-365");
        item.Status = OccupancyDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(OccupancyDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        OccupancyDigestStartsInIdle();
        OccupancyDigestMovesToReady();
        OccupancyDigestRejectsMovesFromStopped();
    }
}
