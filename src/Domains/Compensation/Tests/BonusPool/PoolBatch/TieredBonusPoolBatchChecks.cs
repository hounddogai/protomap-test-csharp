namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Checks of pool batch code, run by the project's runner.</summary>
public static class TieredBonusPoolBatchChecks
{
    public static void TieredBonusPoolBatchStartsInCreated()
    {
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("code-832", "name-910");
        Check.Equal(item.Status, TieredBonusPoolBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredBonusPoolBatchMovesToRetired()
    {
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("code-832", "name-910");
        try
        {
            item.MoveTo(TieredBonusPoolBatchStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredBonusPoolBatchStatus.Retired, "status");
    }

    public static void TieredBonusPoolBatchRejectsMovesFromRetired()
    {
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("code-832", "name-910");
        item.Status = TieredBonusPoolBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredBonusPoolBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredBonusPoolBatchStartsInCreated();
        TieredBonusPoolBatchMovesToRetired();
        TieredBonusPoolBatchRejectsMovesFromRetired();
    }
}
