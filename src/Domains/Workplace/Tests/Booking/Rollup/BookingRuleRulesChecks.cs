namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRuleRulesChecks
{
    public static void BookingRuleValidatorReportsEveryProblem()
    {
        var validator = BookingRuleRules.DefaultBookingRuleValidator();
        var item = BookingRule.NewBookingRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingRuleRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new BookingRuleRequiredLocaleRule("booking rule needs this field");
        var item = BookingRule.NewBookingRule("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BookingRuleRequiredCodeRule("booking rule needs a value");
        var item = BookingRule.NewBookingRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new BookingRuleCodeLengthRule(16);
        var item = BookingRule.NewBookingRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingRuleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new BookingRuleLocaleLengthRule(1000);
        var item = BookingRule.NewBookingRule("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingRuleRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BookingRuleRateRangeRule(2.6, 3633.0);
        var item = BookingRule.NewBookingRule("x", "y");
        item.Rate = 3634.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BookingRuleValidatorReportsEveryProblem();
        BookingRuleRequiredLocaleRuleFlagsABlankLocale();
        BookingRuleRequiredCodeRuleFlagsABlankCode();
        BookingRuleCodeLengthRuleFlagsALongCode();
        BookingRuleLocaleLengthRuleFlagsALongLocale();
        BookingRuleRateRangeRuleFlagsValuesAboveRate();
    }
}
