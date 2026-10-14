namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class PrimaryOccupancyManifestChecks
{
    public static void PrimaryOccupancyManifestStartsInIdle()
    {
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("code-196", "name-312");
        Check.Equal(item.Status, PrimaryOccupancyManifestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryOccupancyManifestMovesToDraining()
    {
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("code-196", "name-312");
        try
        {
            item.MoveTo(PrimaryOccupancyManifestStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryOccupancyManifestStatus.Draining, "status");
    }

    public static void PrimaryOccupancyManifestRejectsMovesFromStopped()
    {
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("code-196", "name-312");
        item.Status = PrimaryOccupancyManifestStatus.Stopped;
        Check.Fails(() => item.MoveTo(PrimaryOccupancyManifestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PrimaryOccupancyManifestStartsInIdle();
        PrimaryOccupancyManifestMovesToDraining();
        PrimaryOccupancyManifestRejectsMovesFromStopped();
    }
}
