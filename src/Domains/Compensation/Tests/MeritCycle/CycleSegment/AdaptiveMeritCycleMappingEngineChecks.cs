namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleMappingEngineChecks
{
    public static void AdaptiveMeritCycleMappingEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveMeritCycleMappingEngine.DefaultAdaptiveMeritCycleMappingEngine();
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("code-243", "name-690");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveMeritCycleMappingEngineKeepsResultsWithinBounds();
    }
}
