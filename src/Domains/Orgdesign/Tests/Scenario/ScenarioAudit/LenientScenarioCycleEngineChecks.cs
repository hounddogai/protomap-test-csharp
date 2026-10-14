namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class LenientScenarioCycleEngineChecks
{
    public static void LenientScenarioCycleEngineKeepsResultsWithinBounds()
    {
        var engine = LenientScenarioCycleEngine.DefaultLenientScenarioCycleEngine(Flags.NewRegistry());
        var item = LenientScenarioCycle.NewLenientScenarioCycle("code-797", "label-826");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientScenarioCycleEngineKeepsResultsWithinBounds();
    }
}
