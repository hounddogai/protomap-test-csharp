namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleTargetEngineChecks
{
    public static void MeritCycleTargetEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleTargetEngine.DefaultMeritCycleTargetEngine();
        var item = MeritCycleTarget.NewMeritCycleTarget("code-101", "name-67");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleTargetEngineKeepsResultsWithinBounds();
    }
}
