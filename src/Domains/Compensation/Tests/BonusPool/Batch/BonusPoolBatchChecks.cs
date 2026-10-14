namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolBatchChecks
{
    public static void BonusPoolBatchStartsInNew()
    {
        var item = BonusPoolBatch.NewBonusPoolBatch("code-771", "title-455");
        Check.Equal(item.Status, BonusPoolBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolBatchMovesToFailed()
    {
        var item = BonusPoolBatch.NewBonusPoolBatch("code-771", "title-455");
        try
        {
            item.MoveTo(BonusPoolBatchStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolBatchStatus.Failed, "status");
    }

    public static void BonusPoolBatchRejectsMovesFromFailed()
    {
        var item = BonusPoolBatch.NewBonusPoolBatch("code-771", "title-455");
        item.Status = BonusPoolBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(BonusPoolBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        BonusPoolBatchStartsInNew();
        BonusPoolBatchMovesToFailed();
        BonusPoolBatchRejectsMovesFromFailed();
    }
}
