namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioScoreEngineChecks
{
    public static void ScenarioScoreEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioScoreEngine.DefaultScenarioScoreEngine(Flags.NewRegistry());
        var item = ScenarioScore.NewScenarioScore("code-437", "name-680");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioScoreEngineKeepsResultsWithinBounds();
    }
}
