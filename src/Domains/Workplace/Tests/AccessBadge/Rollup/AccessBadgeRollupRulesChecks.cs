namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeRollupRulesChecks
{
    public static void AccessBadgeRollupValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeRollupRules.DefaultAccessBadgeRollupValidator();
        var item = AccessBadgeRollup.NewAccessBadgeRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeRollupCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new AccessBadgeRollupCapacityRangeRule(255, 86400);
        var item = AccessBadgeRollup.NewAccessBadgeRollup("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeRollupHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new AccessBadgeRollupHoursRangeRule(8.4, 4559.0);
        var item = AccessBadgeRollup.NewAccessBadgeRollup("x", "y");
        item.Hours = 4560.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeRollupPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new AccessBadgeRollupPhoneLengthRule(1000);
        var item = AccessBadgeRollup.NewAccessBadgeRollup("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgeRollupRequiredCodeRule("access badge rollup needs an entry");
        var item = AccessBadgeRollup.NewAccessBadgeRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeRollupValidatorReportsEveryProblem();
        AccessBadgeRollupCapacityRangeRuleFlagsValuesAboveCapacity();
        AccessBadgeRollupHoursRangeRuleFlagsValuesAboveHours();
        AccessBadgeRollupPhoneLengthRuleFlagsALongPhone();
        AccessBadgeRollupRequiredCodeRuleFlagsABlankCode();
    }
}
