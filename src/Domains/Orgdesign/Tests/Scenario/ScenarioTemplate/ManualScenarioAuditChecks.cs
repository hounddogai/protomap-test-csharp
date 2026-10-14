namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class ManualScenarioAuditChecks
{
    public static void ManualScenarioAuditStartsInNew()
    {
        var item = ManualScenarioAudit.NewManualScenarioAudit("code-86", "title-274");
        Check.Equal(item.Status, ManualScenarioAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualScenarioAuditMovesToRunning()
    {
        var item = ManualScenarioAudit.NewManualScenarioAudit("code-86", "title-274");
        try
        {
            item.MoveTo(ManualScenarioAuditStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualScenarioAuditStatus.Running, "status");
    }

    public static void ManualScenarioAuditRejectsMovesFromFailed()
    {
        var item = ManualScenarioAudit.NewManualScenarioAudit("code-86", "title-274");
        item.Status = ManualScenarioAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualScenarioAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualScenarioAuditStartsInNew();
        ManualScenarioAuditMovesToRunning();
        ManualScenarioAuditRejectsMovesFromFailed();
    }
}
