namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LaptopGroupChecks
{
    public static void LaptopGroupStartsInProposed()
    {
        var item = LaptopGroup.NewLaptopGroup("code-667", "name-933");
        Check.Equal(item.Status, LaptopGroupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopGroupMovesToRejected()
    {
        var item = LaptopGroup.NewLaptopGroup("code-667", "name-933");
        try
        {
            item.MoveTo(LaptopGroupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopGroupStatus.Rejected, "status");
    }

    public static void LaptopGroupRejectsMovesFromWithdrawn()
    {
        var item = LaptopGroup.NewLaptopGroup("code-667", "name-933");
        item.Status = LaptopGroupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LaptopGroupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LaptopGroupStartsInProposed();
        LaptopGroupMovesToRejected();
        LaptopGroupRejectsMovesFromWithdrawn();
    }
}
