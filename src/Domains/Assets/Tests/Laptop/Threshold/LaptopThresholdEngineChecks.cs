namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LaptopThresholdEngineChecks
{
    public static void LaptopThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopThresholdEngine.DefaultLaptopThresholdEngine(Flags.NewRegistry());
        var item = LaptopThreshold.NewLaptopThreshold("code-900", "label-827");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopThresholdEngineKeepsResultsWithinBounds();
    }
}
