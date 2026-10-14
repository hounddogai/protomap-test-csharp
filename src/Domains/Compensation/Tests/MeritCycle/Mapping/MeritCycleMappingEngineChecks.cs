namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class MeritCycleMappingEngineChecks
{
    public static void MeritCycleMappingEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleMappingEngine.DefaultMeritCycleMappingEngine(Flags.NewRegistry());
        var item = MeritCycleMapping.NewMeritCycleMapping("code-48", "title-384");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleMappingEngineKeepsResultsWithinBounds();
    }
}
