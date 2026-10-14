namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopAdjustmentEngineChecks
{
    public static void LaptopAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopAdjustmentEngine.DefaultLaptopAdjustmentEngine(Flags.NewRegistry());
        var item = LaptopAdjustment.NewLaptopAdjustment("code-881", "title-835");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopAdjustmentEngineKeepsResultsWithinBounds();
    }
}
