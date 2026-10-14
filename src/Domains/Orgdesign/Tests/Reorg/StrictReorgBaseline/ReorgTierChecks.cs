namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class ReorgTierChecks
{
    public static void ReorgTierStartsInIdle()
    {
        var item = ReorgTier.NewReorgTier("code-827", "title-418");
        Check.Equal(item.Status, ReorgTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgTierMovesToDraining()
    {
        var item = ReorgTier.NewReorgTier("code-827", "title-418");
        try
        {
            item.MoveTo(ReorgTierStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgTierStatus.Draining, "status");
    }

    public static void ReorgTierRejectsMovesFromStopped()
    {
        var item = ReorgTier.NewReorgTier("code-827", "title-418");
        item.Status = ReorgTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReorgTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReorgTierStartsInIdle();
        ReorgTierMovesToDraining();
        ReorgTierRejectsMovesFromStopped();
    }
}
