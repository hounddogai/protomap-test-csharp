namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LaptopGroupEngineChecks
{
    public static void LaptopGroupEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopGroupEngine.DefaultLaptopGroupEngine(Flags.NewRegistry());
        var item = LaptopGroup.NewLaptopGroup("code-667", "name-933");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopGroupEngineKeepsResultsWithinBounds();
    }
}
