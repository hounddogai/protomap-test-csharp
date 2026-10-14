namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DefaultReorgTierChecks
{
    public static void DefaultReorgTierStartsInProposed()
    {
        var item = DefaultReorgTier.NewDefaultReorgTier("code-974", "label-487");
        Check.Equal(item.Status, DefaultReorgTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultReorgTierMovesToRejected()
    {
        var item = DefaultReorgTier.NewDefaultReorgTier("code-974", "label-487");
        try
        {
            item.MoveTo(DefaultReorgTierStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultReorgTierStatus.Rejected, "status");
    }

    public static void DefaultReorgTierRejectsMovesFromWithdrawn()
    {
        var item = DefaultReorgTier.NewDefaultReorgTier("code-974", "label-487");
        item.Status = DefaultReorgTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultReorgTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultReorgTierStartsInProposed();
        DefaultReorgTierMovesToRejected();
        DefaultReorgTierRejectsMovesFromWithdrawn();
    }
}
