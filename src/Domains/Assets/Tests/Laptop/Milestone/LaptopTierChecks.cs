namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopTierChecks
{
    public static void LaptopTierStartsInProposed()
    {
        var item = LaptopTier.NewLaptopTier("code-469", "title-319");
        Check.Equal(item.Status, LaptopTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopTierMovesToRejected()
    {
        var item = LaptopTier.NewLaptopTier("code-469", "title-319");
        try
        {
            item.MoveTo(LaptopTierStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopTierStatus.Rejected, "status");
    }

    public static void LaptopTierRejectsMovesFromWithdrawn()
    {
        var item = LaptopTier.NewLaptopTier("code-469", "title-319");
        item.Status = LaptopTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LaptopTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LaptopTierStartsInProposed();
        LaptopTierMovesToRejected();
        LaptopTierRejectsMovesFromWithdrawn();
    }
}
