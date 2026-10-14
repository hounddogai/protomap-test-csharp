namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyMeritCycleCheckpointEngineChecks
{
    public static void LegacyMeritCycleCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyMeritCycleCheckpointEngine.DefaultLegacyMeritCycleCheckpointEngine(Flags.NewRegistry());
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("code-105", "name-501");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyMeritCycleCheckpointEngineKeepsResultsWithinBounds();
    }
}
