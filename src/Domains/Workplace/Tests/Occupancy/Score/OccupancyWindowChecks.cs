namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyWindowChecks
{
    public static void OccupancyWindowStartsInNew()
    {
        var item = OccupancyWindow.NewOccupancyWindow("code-605", "label-682");
        Check.Equal(item.Status, OccupancyWindowStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyWindowMovesToQueued()
    {
        var item = OccupancyWindow.NewOccupancyWindow("code-605", "label-682");
        try
        {
            item.MoveTo(OccupancyWindowStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyWindowStatus.Queued, "status");
    }

    public static void OccupancyWindowRejectsMovesFromFailed()
    {
        var item = OccupancyWindow.NewOccupancyWindow("code-605", "label-682");
        item.Status = OccupancyWindowStatus.Failed;
        Check.Fails(() => item.MoveTo(OccupancyWindowStatus.New), "move");
    }

    public static void RunAll()
    {
        OccupancyWindowStartsInNew();
        OccupancyWindowMovesToQueued();
        OccupancyWindowRejectsMovesFromFailed();
    }
}
