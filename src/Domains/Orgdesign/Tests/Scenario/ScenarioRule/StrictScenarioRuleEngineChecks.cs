namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class StrictScenarioRuleEngineChecks
{
    public static void StrictScenarioRuleEngineKeepsResultsWithinBounds()
    {
        var engine = StrictScenarioRuleEngine.DefaultStrictScenarioRuleEngine(Flags.NewRegistry());
        var item = StrictScenarioRule.NewStrictScenarioRule("code-997", "label-909");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictScenarioRuleEngineKeepsResultsWithinBounds();
    }
}
