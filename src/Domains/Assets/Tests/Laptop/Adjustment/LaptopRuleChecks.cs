namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopRuleChecks
{
    public static void LaptopRuleStartsInIdle()
    {
        var item = LaptopRule.NewLaptopRule("code-427", "title-649");
        Check.Equal(item.Status, LaptopRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopRuleMovesToDraining()
    {
        var item = LaptopRule.NewLaptopRule("code-427", "title-649");
        try
        {
            item.MoveTo(LaptopRuleStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopRuleStatus.Draining, "status");
    }

    public static void LaptopRuleRejectsMovesFromStopped()
    {
        var item = LaptopRule.NewLaptopRule("code-427", "title-649");
        item.Status = LaptopRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(LaptopRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LaptopRuleStartsInIdle();
        LaptopRuleMovesToDraining();
        LaptopRuleRejectsMovesFromStopped();
    }
}
