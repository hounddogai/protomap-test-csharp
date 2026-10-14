namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolRollupChecks
{
    public static void BonusPoolRollupStartsInPlanned()
    {
        var item = BonusPoolRollup.NewBonusPoolRollup("code-533", "name-818");
        Check.Equal(item.Status, BonusPoolRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolRollupMovesToCompleted()
    {
        var item = BonusPoolRollup.NewBonusPoolRollup("code-533", "name-818");
        try
        {
            item.MoveTo(BonusPoolRollupStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolRollupStatus.Completed, "status");
    }

    public static void BonusPoolRollupRejectsMovesFromCancelled()
    {
        var item = BonusPoolRollup.NewBonusPoolRollup("code-533", "name-818");
        item.Status = BonusPoolRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BonusPoolRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BonusPoolRollupStartsInPlanned();
        BonusPoolRollupMovesToCompleted();
        BonusPoolRollupRejectsMovesFromCancelled();
    }
}
