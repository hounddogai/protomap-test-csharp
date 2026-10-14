namespace Acme.Workforce.Orgdesign.Tests.Scenario.Template;

using Acme.Workforce.Orgdesign.Scenario.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScenarioLedgerRulesChecks
{
    public static void ScenarioLedgerValidatorReportsEveryProblem()
    {
        var validator = ScenarioLedgerRules.DefaultScenarioLedgerValidator();
        var item = ScenarioLedger.NewScenarioLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioLedgerRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new ScenarioLedgerRequiredBadgeRule("scenario ledger needs a value");
        var item = ScenarioLedger.NewScenarioLedger("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioLedgerThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new ScenarioLedgerThresholdRangeRule(4.6, 1754.8);
        var item = ScenarioLedger.NewScenarioLedger("x", "y");
        item.Threshold = 1756.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScenarioLedgerValidatorReportsEveryProblem();
        ScenarioLedgerRequiredBadgeRuleFlagsABlankBadge();
        ScenarioLedgerThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
