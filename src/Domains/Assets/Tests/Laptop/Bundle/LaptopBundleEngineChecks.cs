namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopBundleEngineChecks
{
    public static void LaptopBundleEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopBundleEngine.DefaultLaptopBundleEngine(Flags.NewRegistry());
        var item = LaptopBundle.NewLaptopBundle("code-510", "title-454");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopBundleEngineKeepsResultsWithinBounds();
    }
}
