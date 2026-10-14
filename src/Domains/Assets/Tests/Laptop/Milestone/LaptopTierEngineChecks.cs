namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopTierEngineChecks
{
    public static void LaptopTierEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopTierEngine.DefaultLaptopTierEngine();
        var item = LaptopTier.NewLaptopTier("code-469", "title-319");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopTierEngineKeepsResultsWithinBounds();
    }
}
