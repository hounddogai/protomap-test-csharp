namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopPlanChecks
{
    public static void LaptopPlanStartsInIdle()
    {
        var item = LaptopPlan.NewLaptopPlan("code-678", "name-144");
        Check.Equal(item.Status, LaptopPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopPlanMovesToWarming()
    {
        var item = LaptopPlan.NewLaptopPlan("code-678", "name-144");
        try
        {
            item.MoveTo(LaptopPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopPlanStatus.Warming, "status");
    }

    public static void LaptopPlanRejectsMovesFromStopped()
    {
        var item = LaptopPlan.NewLaptopPlan("code-678", "name-144");
        item.Status = LaptopPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(LaptopPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LaptopPlanStartsInIdle();
        LaptopPlanMovesToWarming();
        LaptopPlanRejectsMovesFromStopped();
    }
}
