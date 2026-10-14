namespace Acme.Workforce.Workplace.Tests.Occupancy.Review;

using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class OccupancyBatchChecks
{
    public static void OccupancyBatchStartsInOpen()
    {
        var item = OccupancyBatch.NewOccupancyBatch("code-264", "label-146");
        Check.Equal(item.Status, OccupancyBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyBatchMovesToReopened()
    {
        var item = OccupancyBatch.NewOccupancyBatch("code-264", "label-146");
        try
        {
            item.MoveTo(OccupancyBatchStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyBatchStatus.Reopened, "status");
    }

    public static void OccupancyBatchRejectsMovesFromArchived()
    {
        var item = OccupancyBatch.NewOccupancyBatch("code-264", "label-146");
        item.Status = OccupancyBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(OccupancyBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        OccupancyBatchStartsInOpen();
        OccupancyBatchMovesToReopened();
        OccupancyBatchRejectsMovesFromArchived();
    }
}
