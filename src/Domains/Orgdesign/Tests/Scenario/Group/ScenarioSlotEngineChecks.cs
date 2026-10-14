namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScenarioSlotEngineChecks
{
    public static void ScenarioSlotEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioSlotEngine.DefaultScenarioSlotEngine();
        var item = ScenarioSlot.NewScenarioSlot("code-853", "name-763");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioSlotEngineKeepsResultsWithinBounds();
    }
}
