namespace Acme.Workforce.Workplace.Tests.Occupancy.Metric;

using Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RegionalOccupancyDigestChecks
{
    public static void RegionalOccupancyDigestStartsInOpen()
    {
        var item = RegionalOccupancyDigest.NewRegionalOccupancyDigest("code-309", "title-521");
        Check.Equal(item.Status, RegionalOccupancyDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalOccupancyDigestMovesToResolved()
    {
        var item = RegionalOccupancyDigest.NewRegionalOccupancyDigest("code-309", "title-521");
        try
        {
            item.MoveTo(RegionalOccupancyDigestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalOccupancyDigestStatus.Resolved, "status");
    }

    public static void RegionalOccupancyDigestRejectsMovesFromArchived()
    {
        var item = RegionalOccupancyDigest.NewRegionalOccupancyDigest("code-309", "title-521");
        item.Status = RegionalOccupancyDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalOccupancyDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalOccupancyDigestStartsInOpen();
        RegionalOccupancyDigestMovesToResolved();
        RegionalOccupancyDigestRejectsMovesFromArchived();
    }
}
