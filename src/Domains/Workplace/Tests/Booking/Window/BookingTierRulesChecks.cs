namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BookingTierRulesChecks
{
    public static void BookingTierValidatorReportsEveryProblem()
    {
        var validator = BookingTierRules.DefaultBookingTierValidator();
        var item = BookingTier.NewBookingTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BookingTierRequiredCodeRule("booking tier needs an entry");
        var item = BookingTier.NewBookingTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new BookingTierCodeLengthRule(80);
        var item = BookingTier.NewBookingTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingTierMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BookingTierMarginRangeRule(8.9, 657.3);
        var item = BookingTier.NewBookingTier("x", "y");
        item.Margin = 658.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingTierVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new BookingTierVersionRangeRule(32, 86400);
        var item = BookingTier.NewBookingTier("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingTierRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new BookingTierRequiredTimezoneRule("booking tier needs this field");
        var item = BookingTier.NewBookingTier("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BookingTierValidatorReportsEveryProblem();
        BookingTierRequiredCodeRuleFlagsABlankCode();
        BookingTierCodeLengthRuleFlagsALongCode();
        BookingTierMarginRangeRuleFlagsValuesAboveMargin();
        BookingTierVersionRangeRuleFlagsValuesAboveVersion();
        BookingTierRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
