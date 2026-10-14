namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictWarrantyTierChecks
{
    public static void StrictWarrantyTierStartsInProposed()
    {
        var item = StrictWarrantyTier.NewStrictWarrantyTier("code-104", "name-678");
        Check.Equal(item.Status, StrictWarrantyTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictWarrantyTierMovesToWithdrawn()
    {
        var item = StrictWarrantyTier.NewStrictWarrantyTier("code-104", "name-678");
        try
        {
            item.MoveTo(StrictWarrantyTierStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictWarrantyTierStatus.Withdrawn, "status");
    }

    public static void StrictWarrantyTierRejectsMovesFromWithdrawn()
    {
        var item = StrictWarrantyTier.NewStrictWarrantyTier("code-104", "name-678");
        item.Status = StrictWarrantyTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictWarrantyTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictWarrantyTierStartsInProposed();
        StrictWarrantyTierMovesToWithdrawn();
        StrictWarrantyTierRejectsMovesFromWithdrawn();
    }
}
