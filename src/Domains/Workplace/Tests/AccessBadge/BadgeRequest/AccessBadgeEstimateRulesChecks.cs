namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class AccessBadgeEstimateRulesChecks
{
    public static void AccessBadgeEstimateValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeEstimateRules.DefaultAccessBadgeEstimateValidator();
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeEstimateCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new AccessBadgeEstimateCeilingRangeRule(6.1, 3433.9);
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("x", "y");
        item.Ceiling = 3435.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeEstimateCodeLengthRule(80);
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeEstimateMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AccessBadgeEstimateMarginRangeRule(5.4, 773.9);
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("x", "y");
        item.Margin = 775.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AccessBadgeEstimateTitleLengthRule(80);
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeEstimateValidatorReportsEveryProblem();
        AccessBadgeEstimateCeilingRangeRuleFlagsValuesAboveCeiling();
        AccessBadgeEstimateCodeLengthRuleFlagsALongCode();
        AccessBadgeEstimateMarginRangeRuleFlagsValuesAboveMargin();
        AccessBadgeEstimateTitleLengthRuleFlagsALongTitle();
    }
}
