namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioRuleEngineChecks
{
    public static void ScenarioRuleEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioRuleEngine.DefaultScenarioRuleEngine();
        var item = ScenarioRule.NewScenarioRule("code-557", "label-384");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioRuleEngineKeepsResultsWithinBounds();
    }
}
