namespace Acme.Workforce.Assets.Tests.Laptop.Window;

using Acme.Workforce.Assets.Laptop.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredLaptopWindowChecks
{
    public static void TieredLaptopWindowStartsInProposed()
    {
        var item = TieredLaptopWindow.NewTieredLaptopWindow("code-944", "label-795");
        Check.Equal(item.Status, TieredLaptopWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLaptopWindowMovesToWithdrawn()
    {
        var item = TieredLaptopWindow.NewTieredLaptopWindow("code-944", "label-795");
        try
        {
            item.MoveTo(TieredLaptopWindowStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLaptopWindowStatus.Withdrawn, "status");
    }

    public static void TieredLaptopWindowRejectsMovesFromWithdrawn()
    {
        var item = TieredLaptopWindow.NewTieredLaptopWindow("code-944", "label-795");
        item.Status = TieredLaptopWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredLaptopWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredLaptopWindowStartsInProposed();
        TieredLaptopWindowMovesToWithdrawn();
        TieredLaptopWindowRejectsMovesFromWithdrawn();
    }
}
