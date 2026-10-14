namespace Acme.Workforce.Orgdesign.Tests.Scenario.Template;

using Acme.Workforce.Orgdesign.Scenario.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScenarioLedgerEngineChecks
{
    public static void ScenarioLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioLedgerEngine.DefaultScenarioLedgerEngine();
        var item = ScenarioLedger.NewScenarioLedger("code-799", "title-328");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioLedgerEngineKeepsResultsWithinBounds();
    }
}
