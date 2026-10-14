namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class OccupancyEstimateChecks
{
    public static void OccupancyEstimateStartsInNew()
    {
        var item = OccupancyEstimate.NewOccupancyEstimate("code-105", "title-460");
        Check.Equal(item.Status, OccupancyEstimateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyEstimateMovesToRunning()
    {
        var item = OccupancyEstimate.NewOccupancyEstimate("code-105", "title-460");
        try
        {
            item.MoveTo(OccupancyEstimateStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyEstimateStatus.Running, "status");
    }

    public static void OccupancyEstimateRejectsMovesFromFailed()
    {
        var item = OccupancyEstimate.NewOccupancyEstimate("code-105", "title-460");
        item.Status = OccupancyEstimateStatus.Failed;
        Check.Fails(() => item.MoveTo(OccupancyEstimateStatus.New), "move");
    }

    public static void RunAll()
    {
        OccupancyEstimateStartsInNew();
        OccupancyEstimateMovesToRunning();
        OccupancyEstimateRejectsMovesFromFailed();
    }
}
