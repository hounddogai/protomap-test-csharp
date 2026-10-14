namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioSummaryEngineChecks
{
    public static void ScenarioSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioSummaryEngine.DefaultScenarioSummaryEngine(Flags.NewRegistry());
        var item = ScenarioSummary.NewScenarioSummary("code-357", "title-208");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioSummaryEngineKeepsResultsWithinBounds();
    }
}
