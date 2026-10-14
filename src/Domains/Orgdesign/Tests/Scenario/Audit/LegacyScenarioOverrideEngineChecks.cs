namespace Acme.Workforce.Orgdesign.Tests.Scenario.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyScenarioOverrideEngineChecks
{
    public static void LegacyScenarioOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyScenarioOverrideEngine.DefaultLegacyScenarioOverrideEngine(Flags.NewRegistry());
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("code-298", "title-53");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyScenarioOverrideEngineKeepsResultsWithinBounds();
    }
}
