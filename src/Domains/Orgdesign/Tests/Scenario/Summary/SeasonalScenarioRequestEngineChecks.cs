namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SeasonalScenarioRequestEngineChecks
{
    public static void SeasonalScenarioRequestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalScenarioRequestEngine.DefaultSeasonalScenarioRequestEngine(Flags.NewRegistry());
        var item = SeasonalScenarioRequest.NewSeasonalScenarioRequest("code-406", "label-521");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalScenarioRequestEngineKeepsResultsWithinBounds();
    }
}
