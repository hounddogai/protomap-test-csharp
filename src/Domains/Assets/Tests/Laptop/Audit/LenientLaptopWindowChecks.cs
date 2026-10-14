namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LenientLaptopWindowChecks
{
    public static void LenientLaptopWindowStartsInIdle()
    {
        var item = LenientLaptopWindow.NewLenientLaptopWindow("code-360", "name-368");
        Check.Equal(item.Status, LenientLaptopWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLaptopWindowMovesToDraining()
    {
        var item = LenientLaptopWindow.NewLenientLaptopWindow("code-360", "name-368");
        try
        {
            item.MoveTo(LenientLaptopWindowStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLaptopWindowStatus.Draining, "status");
    }

    public static void LenientLaptopWindowRejectsMovesFromStopped()
    {
        var item = LenientLaptopWindow.NewLenientLaptopWindow("code-360", "name-368");
        item.Status = LenientLaptopWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientLaptopWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientLaptopWindowStartsInIdle();
        LenientLaptopWindowMovesToDraining();
        LenientLaptopWindowRejectsMovesFromStopped();
    }
}
