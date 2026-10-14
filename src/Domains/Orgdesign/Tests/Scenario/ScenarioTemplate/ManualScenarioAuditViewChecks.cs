namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class ManualScenarioAuditViewChecks
{
    public static void ManualScenarioAuditViewShowsTheStatusInCapitals()
    {
        var item = ManualScenarioAudit.NewManualScenarioAudit("code-86", "title-274");
        var view = ManualScenarioAuditView.ToManualScenarioAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualScenarioAuditView.ToManualScenarioAuditViews(new List<ManualScenarioAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualScenarioAuditViewShowsTheStatusInCapitals();
    }
}
