namespace Acme.Workforce.Orgdesign.Tests.Scenario.Reservation;

using Acme.Workforce.Orgdesign.Scenario.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScenarioMilestoneEngineChecks
{
    public static void ScenarioMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioMilestoneEngine.DefaultScenarioMilestoneEngine();
        var item = ScenarioMilestone.NewScenarioMilestone("code-309", "title-880");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioMilestoneEngineKeepsResultsWithinBounds();
    }
}
