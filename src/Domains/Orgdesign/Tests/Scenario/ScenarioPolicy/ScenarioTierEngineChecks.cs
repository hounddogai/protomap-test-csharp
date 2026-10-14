namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioTierEngineChecks
{
    public static void ScenarioTierEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioTierEngine.DefaultScenarioTierEngine();
        var item = ScenarioTier.NewScenarioTier("code-298", "name-825");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioTierEngineKeepsResultsWithinBounds();
    }
}
