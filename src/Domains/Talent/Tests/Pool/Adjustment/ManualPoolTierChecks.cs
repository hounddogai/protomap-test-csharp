namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualPoolTierChecks
{
    public static void ManualPoolTierStartsInProposed()
    {
        var item = ManualPoolTier.NewManualPoolTier("code-855", "name-647");
        Check.Equal(item.Status, ManualPoolTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPoolTierMovesToWithdrawn()
    {
        var item = ManualPoolTier.NewManualPoolTier("code-855", "name-647");
        try
        {
            item.MoveTo(ManualPoolTierStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPoolTierStatus.Withdrawn, "status");
    }

    public static void ManualPoolTierRejectsMovesFromWithdrawn()
    {
        var item = ManualPoolTier.NewManualPoolTier("code-855", "name-647");
        item.Status = ManualPoolTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualPoolTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualPoolTierStartsInProposed();
        ManualPoolTierMovesToWithdrawn();
        ManualPoolTierRejectsMovesFromWithdrawn();
    }
}
