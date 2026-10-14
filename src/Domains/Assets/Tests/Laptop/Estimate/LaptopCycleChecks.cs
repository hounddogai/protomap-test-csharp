namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LaptopCycleChecks
{
    public static void LaptopCycleStartsInRequested()
    {
        var item = LaptopCycle.NewLaptopCycle("code-221", "name-183");
        Check.Equal(item.Status, LaptopCycleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopCycleMovesToRevoked()
    {
        var item = LaptopCycle.NewLaptopCycle("code-221", "name-183");
        try
        {
            item.MoveTo(LaptopCycleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopCycleStatus.Revoked, "status");
    }

    public static void LaptopCycleRejectsMovesFromExpired()
    {
        var item = LaptopCycle.NewLaptopCycle("code-221", "name-183");
        item.Status = LaptopCycleStatus.Expired;
        Check.Fails(() => item.MoveTo(LaptopCycleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LaptopCycleStartsInRequested();
        LaptopCycleMovesToRevoked();
        LaptopCycleRejectsMovesFromExpired();
    }
}
