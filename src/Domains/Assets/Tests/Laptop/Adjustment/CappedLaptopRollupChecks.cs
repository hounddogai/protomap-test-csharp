namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedLaptopRollupChecks
{
    public static void CappedLaptopRollupStartsInProposed()
    {
        var item = CappedLaptopRollup.NewCappedLaptopRollup("code-489", "title-541");
        Check.Equal(item.Status, CappedLaptopRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedLaptopRollupMovesToRejected()
    {
        var item = CappedLaptopRollup.NewCappedLaptopRollup("code-489", "title-541");
        try
        {
            item.MoveTo(CappedLaptopRollupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedLaptopRollupStatus.Rejected, "status");
    }

    public static void CappedLaptopRollupRejectsMovesFromWithdrawn()
    {
        var item = CappedLaptopRollup.NewCappedLaptopRollup("code-489", "title-541");
        item.Status = CappedLaptopRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedLaptopRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedLaptopRollupStartsInProposed();
        CappedLaptopRollupMovesToRejected();
        CappedLaptopRollupRejectsMovesFromWithdrawn();
    }
}
