namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class BatchedPositionEstimateChecks
{
    public static void BatchedPositionEstimateStartsInIdle()
    {
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("code-742", "name-928");
        Check.Equal(item.Status, BatchedPositionEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPositionEstimateMovesToReady()
    {
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("code-742", "name-928");
        try
        {
            item.MoveTo(BatchedPositionEstimateStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPositionEstimateStatus.Ready, "status");
    }

    public static void BatchedPositionEstimateRejectsMovesFromStopped()
    {
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("code-742", "name-928");
        item.Status = BatchedPositionEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedPositionEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedPositionEstimateStartsInIdle();
        BatchedPositionEstimateMovesToReady();
        BatchedPositionEstimateRejectsMovesFromStopped();
    }
}
