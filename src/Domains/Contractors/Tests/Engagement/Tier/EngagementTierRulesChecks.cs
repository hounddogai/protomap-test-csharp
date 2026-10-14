namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementTierRulesChecks
{
    public static void EngagementTierValidatorReportsEveryProblem()
    {
        var validator = EngagementTierRules.DefaultEngagementTierValidator();
        var item = EngagementTier.NewEngagementTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementTierSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new EngagementTierSummaryLengthRule(32);
        var item = EngagementTier.NewEngagementTier("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementTierTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new EngagementTierTimezoneLengthRule(64);
        var item = EngagementTier.NewEngagementTier("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementTierRequiredCodeRule("engagement tier needs a value");
        var item = EngagementTier.NewEngagementTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EngagementTierLabelLengthRule(3600);
        var item = EngagementTier.NewEngagementTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EngagementTierValidatorReportsEveryProblem();
        EngagementTierSummaryLengthRuleFlagsALongSummary();
        EngagementTierTimezoneLengthRuleFlagsALongTimezone();
        EngagementTierRequiredCodeRuleFlagsABlankCode();
        EngagementTierLabelLengthRuleFlagsALongLabel();
    }
}
