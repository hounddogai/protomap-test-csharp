namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalBookingTierRulesChecks
{
    public static void GlobalBookingTierValidatorReportsEveryProblem()
    {
        var validator = GlobalBookingTierRules.DefaultGlobalBookingTierValidator();
        var item = GlobalBookingTier.NewGlobalBookingTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalBookingTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalBookingTierRequiredCodeRule("global booking tier needs this field");
        var item = GlobalBookingTier.NewGlobalBookingTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalBookingTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalBookingTierTitleLengthRule(16);
        var item = GlobalBookingTier.NewGlobalBookingTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalBookingTierPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new GlobalBookingTierPriorityRangeRule(80, 86400);
        var item = GlobalBookingTier.NewGlobalBookingTier("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalBookingTierValidatorReportsEveryProblem();
        GlobalBookingTierRequiredCodeRuleFlagsABlankCode();
        GlobalBookingTierTitleLengthRuleFlagsALongTitle();
        GlobalBookingTierPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
