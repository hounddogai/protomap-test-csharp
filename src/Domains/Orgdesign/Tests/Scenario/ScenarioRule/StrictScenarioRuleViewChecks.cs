namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class StrictScenarioRuleViewChecks
{
    public static void StrictScenarioRuleViewShowsTheStatusInCapitals()
    {
        var item = StrictScenarioRule.NewStrictScenarioRule("code-997", "label-909");
        var view = StrictScenarioRuleView.ToStrictScenarioRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictScenarioRuleView.ToStrictScenarioRuleViews(new List<StrictScenarioRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictScenarioRuleViewShowsTheStatusInCapitals();
    }
}
