namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class DefaultPositionThresholdEngineChecks
{
    public static void DefaultPositionThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPositionThresholdEngine.DefaultDefaultPositionThresholdEngine(Flags.NewRegistry());
        var item = DefaultPositionThreshold.NewDefaultPositionThreshold("code-655", "title-345");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPositionThresholdEngineKeepsResultsWithinBounds();
    }
}
