namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleMilestoneEngineChecks
{
    public static void MeritCycleMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleMilestoneEngine.DefaultMeritCycleMilestoneEngine();
        var item = MeritCycleMilestone.NewMeritCycleMilestone("code-178", "label-369");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleMilestoneEngineKeepsResultsWithinBounds();
    }
}
