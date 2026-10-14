namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ReorgThresholdEngineChecks
{
    public static void ReorgThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgThresholdEngine.DefaultReorgThresholdEngine(Flags.NewRegistry());
        var item = ReorgThreshold.NewReorgThreshold("code-85", "title-901");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgThresholdEngineKeepsResultsWithinBounds();
    }
}
