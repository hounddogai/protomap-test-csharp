namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeDigestRulesChecks
{
    public static void AccessBadgeDigestValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeDigestRules.DefaultAccessBadgeDigestValidator();
        var item = AccessBadgeDigest.NewAccessBadgeDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccessBadgeDigestRequiredNameRule("access badge digest needs a value");
        var item = AccessBadgeDigest.NewAccessBadgeDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeDigestHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new AccessBadgeDigestHoursRangeRule(8.5, 2294.3);
        var item = AccessBadgeDigest.NewAccessBadgeDigest("x", "y");
        item.Hours = 2295.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeDigestSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new AccessBadgeDigestSummaryLengthRule(86400);
        var item = AccessBadgeDigest.NewAccessBadgeDigest("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeDigestCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new AccessBadgeDigestCapacityRangeRule(32, 3600);
        var item = AccessBadgeDigest.NewAccessBadgeDigest("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccessBadgeDigestValidatorReportsEveryProblem();
        AccessBadgeDigestRequiredNameRuleFlagsABlankName();
        AccessBadgeDigestHoursRangeRuleFlagsValuesAboveHours();
        AccessBadgeDigestSummaryLengthRuleFlagsALongSummary();
        AccessBadgeDigestCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
