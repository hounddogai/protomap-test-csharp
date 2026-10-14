namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioRuleViewChecks
{
    public static void ScenarioRuleViewShowsTheStatusInCapitals()
    {
        var item = ScenarioRule.NewScenarioRule("code-557", "label-384");
        var view = ScenarioRuleView.ToScenarioRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioRuleView.ToScenarioRuleViews(new List<ScenarioRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioRuleViewShowsTheStatusInCapitals();
    }
}
