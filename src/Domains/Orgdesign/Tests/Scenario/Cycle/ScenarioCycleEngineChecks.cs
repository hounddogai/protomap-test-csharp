namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioCycleEngineChecks
{
    public static void ScenarioCycleEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioCycleEngine.DefaultScenarioCycleEngine();
        var item = ScenarioCycle.NewScenarioCycle("code-763", "label-671");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioCycleEngineKeepsResultsWithinBounds();
    }
}
