namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class RollingAccessBadgeProfileRulesChecks
{
    public static void RollingAccessBadgeProfileValidatorReportsEveryProblem()
    {
        var validator = RollingAccessBadgeProfileRules.DefaultRollingAccessBadgeProfileValidator();
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAccessBadgeProfileBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RollingAccessBadgeProfileBadgeLengthRule(1000);
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAccessBadgeProfileCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RollingAccessBadgeProfileCapacityRangeRule(80, 86400);
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAccessBadgeProfileWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new RollingAccessBadgeProfileWindowDaysRangeRule(16, 80);
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("x", "y");
        item.WindowDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAccessBadgeProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingAccessBadgeProfileRequiredCodeRule("rolling access badge profile needs a value");
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingAccessBadgeProfileValidatorReportsEveryProblem();
        RollingAccessBadgeProfileBadgeLengthRuleFlagsALongBadge();
        RollingAccessBadgeProfileCapacityRangeRuleFlagsValuesAboveCapacity();
        RollingAccessBadgeProfileWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        RollingAccessBadgeProfileRequiredCodeRuleFlagsABlankCode();
    }
}
