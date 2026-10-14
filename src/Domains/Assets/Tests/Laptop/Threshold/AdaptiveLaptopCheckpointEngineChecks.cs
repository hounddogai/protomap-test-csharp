namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveLaptopCheckpointEngineChecks
{
    public static void AdaptiveLaptopCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveLaptopCheckpointEngine.DefaultAdaptiveLaptopCheckpointEngine(Flags.NewRegistry());
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("code-968", "label-883");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveLaptopCheckpointEngineKeepsResultsWithinBounds();
    }
}
