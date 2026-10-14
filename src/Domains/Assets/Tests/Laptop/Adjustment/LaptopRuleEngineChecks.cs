namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopRuleEngineChecks
{
    public static void LaptopRuleEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopRuleEngine.DefaultLaptopRuleEngine();
        var item = LaptopRule.NewLaptopRule("code-427", "title-649");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopRuleEngineKeepsResultsWithinBounds();
    }
}
