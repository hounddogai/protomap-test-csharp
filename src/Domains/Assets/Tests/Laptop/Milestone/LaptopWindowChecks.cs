namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopWindowChecks
{
    public static void LaptopWindowStartsInRequested()
    {
        var item = LaptopWindow.NewLaptopWindow("code-917", "name-418");
        Check.Equal(item.Status, LaptopWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopWindowMovesToExpired()
    {
        var item = LaptopWindow.NewLaptopWindow("code-917", "name-418");
        try
        {
            item.MoveTo(LaptopWindowStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopWindowStatus.Expired, "status");
    }

    public static void LaptopWindowRejectsMovesFromExpired()
    {
        var item = LaptopWindow.NewLaptopWindow("code-917", "name-418");
        item.Status = LaptopWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(LaptopWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LaptopWindowStartsInRequested();
        LaptopWindowMovesToExpired();
        LaptopWindowRejectsMovesFromExpired();
    }
}
