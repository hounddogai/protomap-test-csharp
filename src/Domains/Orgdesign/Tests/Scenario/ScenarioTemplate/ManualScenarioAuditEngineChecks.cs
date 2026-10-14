namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class ManualScenarioAuditEngineChecks
{
    public static void ManualScenarioAuditEngineKeepsResultsWithinBounds()
    {
        var engine = ManualScenarioAuditEngine.DefaultManualScenarioAuditEngine(Flags.NewRegistry());
        var item = ManualScenarioAudit.NewManualScenarioAudit("code-86", "title-274");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualScenarioAuditEngineKeepsResultsWithinBounds();
    }
}
