namespace Acme.Workforce.Compensation.Tests.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class MeritCycleGroupEngineChecks
{
    public static void MeritCycleGroupEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleGroupEngine.DefaultMeritCycleGroupEngine(Flags.NewRegistry());
        var item = MeritCycleGroup.NewMeritCycleGroup("code-329", "name-936");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleGroupEngineKeepsResultsWithinBounds();
    }
}
