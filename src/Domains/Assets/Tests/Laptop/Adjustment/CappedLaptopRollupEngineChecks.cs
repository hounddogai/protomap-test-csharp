namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedLaptopRollupEngineChecks
{
    public static void CappedLaptopRollupEngineKeepsResultsWithinBounds()
    {
        var engine = CappedLaptopRollupEngine.DefaultCappedLaptopRollupEngine();
        var item = CappedLaptopRollup.NewCappedLaptopRollup("code-489", "title-541");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedLaptopRollupEngineKeepsResultsWithinBounds();
    }
}
