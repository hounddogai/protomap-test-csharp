namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackMeritCycleCheckpointEngineChecks
{
    public static void FallbackMeritCycleCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackMeritCycleCheckpointEngine.DefaultFallbackMeritCycleCheckpointEngine(Flags.NewRegistry());
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("code-89", "name-799");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackMeritCycleCheckpointEngineKeepsResultsWithinBounds();
    }
}
