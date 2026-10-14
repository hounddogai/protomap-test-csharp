namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioRuleViewChecks
{
    public static void ManualScenarioRuleViewShowsTheStatusInCapitals()
    {
        var item = ManualScenarioRule.NewManualScenarioRule("code-542", "label-479");
        var view = ManualScenarioRuleView.ToManualScenarioRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualScenarioRuleView.ToManualScenarioRuleViews(new List<ManualScenarioRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualScenarioRuleViewShowsTheStatusInCapitals();
    }
}
