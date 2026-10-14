namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalScenarioPolicyEngineChecks
{
    public static void GlobalScenarioPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalScenarioPolicyEngine.DefaultGlobalScenarioPolicyEngine(Flags.NewRegistry());
        var item = GlobalScenarioPolicy.NewGlobalScenarioPolicy("code-231", "name-659");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalScenarioPolicyEngineKeepsResultsWithinBounds();
    }
}
