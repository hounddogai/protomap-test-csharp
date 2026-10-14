namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class ScheduledReorgThresholdEngineChecks
{
    public static void ScheduledReorgThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledReorgThresholdEngine.DefaultScheduledReorgThresholdEngine();
        var item = ScheduledReorgThreshold.NewScheduledReorgThreshold("code-175", "label-482");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledReorgThresholdEngineKeepsResultsWithinBounds();
    }
}
