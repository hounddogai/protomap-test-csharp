namespace Acme.Workforce.Compensation.Tests.MeritCycle.Metric;

using Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredMeritCycleCheckpointEngineChecks
{
    public static void TieredMeritCycleCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = TieredMeritCycleCheckpointEngine.DefaultTieredMeritCycleCheckpointEngine();
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("code-661", "title-457");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredMeritCycleCheckpointEngineKeepsResultsWithinBounds();
    }
}
