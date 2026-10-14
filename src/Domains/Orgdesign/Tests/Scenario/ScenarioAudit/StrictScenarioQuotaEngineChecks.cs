namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class StrictScenarioQuotaEngineChecks
{
    public static void StrictScenarioQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = StrictScenarioQuotaEngine.DefaultStrictScenarioQuotaEngine(Flags.NewRegistry());
        var item = StrictScenarioQuota.NewStrictScenarioQuota("code-509", "name-50");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictScenarioQuotaEngineKeepsResultsWithinBounds();
    }
}
