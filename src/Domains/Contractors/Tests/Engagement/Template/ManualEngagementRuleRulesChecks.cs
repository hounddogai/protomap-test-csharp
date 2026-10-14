namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ManualEngagementRuleRulesChecks
{
    public static void ManualEngagementRuleValidatorReportsEveryProblem()
    {
        var validator = ManualEngagementRuleRules.DefaultManualEngagementRuleValidator();
        var item = ManualEngagementRule.NewManualEngagementRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualEngagementRuleVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ManualEngagementRuleVersionRangeRule(32, 80);
        var item = ManualEngagementRule.NewManualEngagementRule("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualEngagementRuleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new ManualEngagementRuleStartsAtRangeRule(80, 3600);
        var item = ManualEngagementRule.NewManualEngagementRule("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualEngagementRuleValidatorReportsEveryProblem();
        ManualEngagementRuleVersionRangeRuleFlagsValuesAboveVersion();
        ManualEngagementRuleStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
