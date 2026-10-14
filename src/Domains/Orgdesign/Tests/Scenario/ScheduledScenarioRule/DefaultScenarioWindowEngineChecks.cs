namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class DefaultScenarioWindowEngineChecks
{
    public static void DefaultScenarioWindowEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultScenarioWindowEngine.DefaultDefaultScenarioWindowEngine(Flags.NewRegistry());
        var item = DefaultScenarioWindow.NewDefaultScenarioWindow("code-439", "label-870");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultScenarioWindowEngineKeepsResultsWithinBounds();
    }
}
