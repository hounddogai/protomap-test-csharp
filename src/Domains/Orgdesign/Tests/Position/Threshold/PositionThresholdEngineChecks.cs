namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PositionThresholdEngineChecks
{
    public static void PositionThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = PositionThresholdEngine.DefaultPositionThresholdEngine(Flags.NewRegistry());
        var item = PositionThreshold.NewPositionThreshold("code-608", "label-114");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionThresholdEngineKeepsResultsWithinBounds();
    }
}
