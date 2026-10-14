namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioAssignmentEngineChecks
{
    public static void ScenarioAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioAssignmentEngine.DefaultScenarioAssignmentEngine(Flags.NewRegistry());
        var item = ScenarioAssignment.NewScenarioAssignment("code-972", "label-199");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioAssignmentEngineKeepsResultsWithinBounds();
    }
}
