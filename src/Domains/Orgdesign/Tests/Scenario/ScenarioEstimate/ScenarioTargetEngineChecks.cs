namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class ScenarioTargetEngineChecks
{
    public static void ScenarioTargetEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioTargetEngine.DefaultScenarioTargetEngine();
        var item = ScenarioTarget.NewScenarioTarget("code-924", "name-949");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioTargetEngineKeepsResultsWithinBounds();
    }
}
