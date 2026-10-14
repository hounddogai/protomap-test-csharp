namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class MeritCyclePolicyEngineChecks
{
    public static void MeritCyclePolicyEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCyclePolicyEngine.DefaultMeritCyclePolicyEngine();
        var item = MeritCyclePolicy.NewMeritCyclePolicy("code-385", "label-130");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCyclePolicyEngineKeepsResultsWithinBounds();
    }
}
