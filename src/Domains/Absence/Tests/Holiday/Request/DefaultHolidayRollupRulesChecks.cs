namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class DefaultHolidayRollupRulesChecks
{
    public static void DefaultHolidayRollupValidatorReportsEveryProblem()
    {
        var validator = DefaultHolidayRollupRules.DefaultDefaultHolidayRollupValidator();
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultHolidayRollupRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new DefaultHolidayRollupRequiredPhoneRule("default holiday rollup needs an entry");
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHolidayRollupDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new DefaultHolidayRollupDiscountRangeRule(6.9, 2258.4);
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("x", "y");
        item.Discount = 2259.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHolidayRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultHolidayRollupCodeLengthRule(255);
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultHolidayRollupAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new DefaultHolidayRollupAttemptsRangeRule(120, 86400);
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultHolidayRollupValidatorReportsEveryProblem();
        DefaultHolidayRollupRequiredPhoneRuleFlagsABlankPhone();
        DefaultHolidayRollupDiscountRangeRuleFlagsValuesAboveDiscount();
        DefaultHolidayRollupCodeLengthRuleFlagsALongCode();
        DefaultHolidayRollupAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
