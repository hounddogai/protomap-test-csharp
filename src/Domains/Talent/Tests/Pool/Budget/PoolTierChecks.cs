namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolTierChecks
{
    public static void PoolTierStartsInIdle()
    {
        var item = PoolTier.NewPoolTier("code-931", "title-742");
        Check.Equal(item.Status, PoolTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolTierMovesToReady()
    {
        var item = PoolTier.NewPoolTier("code-931", "title-742");
        try
        {
            item.MoveTo(PoolTierStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolTierStatus.Ready, "status");
    }

    public static void PoolTierRejectsMovesFromStopped()
    {
        var item = PoolTier.NewPoolTier("code-931", "title-742");
        item.Status = PoolTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(PoolTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PoolTierStartsInIdle();
        PoolTierMovesToReady();
        PoolTierRejectsMovesFromStopped();
    }
}
