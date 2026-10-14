namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleRollupEngineChecks
{
    public static void MeritCycleRollupEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleRollupEngine.DefaultMeritCycleRollupEngine();
        var item = MeritCycleRollup.NewMeritCycleRollup("code-717", "title-729");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleRollupEngineKeepsResultsWithinBounds();
    }
}
