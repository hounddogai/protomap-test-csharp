namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LegacyOccupancyDigestChecks
{
    public static void LegacyOccupancyDigestStartsInPlanned()
    {
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("code-950", "name-756");
        Check.Equal(item.Status, LegacyOccupancyDigestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyOccupancyDigestMovesToInProgress()
    {
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("code-950", "name-756");
        try
        {
            item.MoveTo(LegacyOccupancyDigestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyOccupancyDigestStatus.InProgress, "status");
    }

    public static void LegacyOccupancyDigestRejectsMovesFromCancelled()
    {
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("code-950", "name-756");
        item.Status = LegacyOccupancyDigestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyOccupancyDigestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyOccupancyDigestStartsInPlanned();
        LegacyOccupancyDigestMovesToInProgress();
        LegacyOccupancyDigestRejectsMovesFromCancelled();
    }
}
