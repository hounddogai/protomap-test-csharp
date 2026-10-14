namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PoolRollupChecks
{
    public static void PoolRollupStartsInNew()
    {
        var item = PoolRollup.NewPoolRollup("code-887", "title-915");
        Check.Equal(item.Status, PoolRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolRollupMovesToQueued()
    {
        var item = PoolRollup.NewPoolRollup("code-887", "title-915");
        try
        {
            item.MoveTo(PoolRollupStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolRollupStatus.Queued, "status");
    }

    public static void PoolRollupRejectsMovesFromFailed()
    {
        var item = PoolRollup.NewPoolRollup("code-887", "title-915");
        item.Status = PoolRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(PoolRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        PoolRollupStartsInNew();
        PoolRollupMovesToQueued();
        PoolRollupRejectsMovesFromFailed();
    }
}
