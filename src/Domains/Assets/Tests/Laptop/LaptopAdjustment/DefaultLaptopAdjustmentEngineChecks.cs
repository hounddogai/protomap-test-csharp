namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class DefaultLaptopAdjustmentEngineChecks
{
    public static void DefaultLaptopAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultLaptopAdjustmentEngine.DefaultDefaultLaptopAdjustmentEngine();
        var item = DefaultLaptopAdjustment.NewDefaultLaptopAdjustment("code-49", "label-636");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultLaptopAdjustmentEngineKeepsResultsWithinBounds();
    }
}
