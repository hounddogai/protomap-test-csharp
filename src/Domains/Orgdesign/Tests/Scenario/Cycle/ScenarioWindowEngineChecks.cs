namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioWindowEngineChecks
{
    public static void ScenarioWindowEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioWindowEngine.DefaultScenarioWindowEngine();
        var item = ScenarioWindow.NewScenarioWindow("code-764", "label-111");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioWindowEngineKeepsResultsWithinBounds();
    }
}
