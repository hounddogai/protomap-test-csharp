namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedBonusPoolThresholdChecks
{
    public static void BatchedBonusPoolThresholdStartsInOpen()
    {
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("code-983", "title-737");
        Check.Equal(item.Status, BatchedBonusPoolThresholdStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedBonusPoolThresholdMovesToReopened()
    {
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("code-983", "title-737");
        try
        {
            item.MoveTo(BatchedBonusPoolThresholdStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedBonusPoolThresholdStatus.Reopened, "status");
    }

    public static void BatchedBonusPoolThresholdRejectsMovesFromArchived()
    {
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("code-983", "title-737");
        item.Status = BatchedBonusPoolThresholdStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedBonusPoolThresholdStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedBonusPoolThresholdStartsInOpen();
        BatchedBonusPoolThresholdMovesToReopened();
        BatchedBonusPoolThresholdRejectsMovesFromArchived();
    }
}
