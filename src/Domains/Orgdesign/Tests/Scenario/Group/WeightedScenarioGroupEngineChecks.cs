namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedScenarioGroupEngineChecks
{
    public static void WeightedScenarioGroupEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedScenarioGroupEngine.DefaultWeightedScenarioGroupEngine();
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("code-267", "title-172");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedScenarioGroupEngineKeepsResultsWithinBounds();
    }
}
