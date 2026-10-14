namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredLaptopBaselineEngineChecks
{
    public static void TieredLaptopBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = TieredLaptopBaselineEngine.DefaultTieredLaptopBaselineEngine(Flags.NewRegistry());
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("code-902", "title-475");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredLaptopBaselineEngineKeepsResultsWithinBounds();
    }
}
