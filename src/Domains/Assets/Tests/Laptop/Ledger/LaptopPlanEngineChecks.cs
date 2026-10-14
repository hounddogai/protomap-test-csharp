namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopPlanEngineChecks
{
    public static void LaptopPlanEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopPlanEngine.DefaultLaptopPlanEngine();
        var item = LaptopPlan.NewLaptopPlan("code-678", "name-144");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopPlanEngineKeepsResultsWithinBounds();
    }
}
