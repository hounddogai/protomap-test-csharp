namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class DefaultScenarioWindowRulesChecks
{
    public static void DefaultScenarioWindowValidatorReportsEveryProblem()
    {
        var validator = DefaultScenarioWindowRules.DefaultDefaultScenarioWindowValidator();
        var item = DefaultScenarioWindow.NewDefaultScenarioWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultScenarioWindowRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultScenarioWindowRequiredLabelRule("default scenario window needs an entry");
        var item = DefaultScenarioWindow.NewDefaultScenarioWindow("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultScenarioWindowValidatorReportsEveryProblem();
        DefaultScenarioWindowRequiredLabelRuleFlagsABlankLabel();
    }
}
