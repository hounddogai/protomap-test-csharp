namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioWindowEngineChecks
{
    public static void ManualScenarioWindowEngineKeepsResultsWithinBounds()
    {
        var engine = ManualScenarioWindowEngine.DefaultManualScenarioWindowEngine();
        var item = ManualScenarioWindow.NewManualScenarioWindow("code-613", "title-689");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualScenarioWindowEngineKeepsResultsWithinBounds();
    }
}
