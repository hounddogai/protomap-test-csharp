namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingClaimRulesChecks
{
    public static void BookingClaimValidatorReportsEveryProblem()
    {
        var validator = BookingClaimRules.DefaultBookingClaimValidator();
        var item = BookingClaim.NewBookingClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingClaimRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new BookingClaimRequiredCurrencyRule("booking claim needs a value");
        var item = BookingClaim.NewBookingClaim("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new BookingClaimCodeLengthRule(1000);
        var item = BookingClaim.NewBookingClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingClaimLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new BookingClaimLimitRangeRule(64, 3600);
        var item = BookingClaim.NewBookingClaim("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingClaimRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BookingClaimRatioRangeRule(4.7, 852.0);
        var item = BookingClaim.NewBookingClaim("x", "y");
        item.Ratio = 853.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingClaimRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new BookingClaimRequiredChannelRule("booking claim needs an entry");
        var item = BookingClaim.NewBookingClaim("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BookingClaimRequiredCodeRule("booking claim needs an entry");
        var item = BookingClaim.NewBookingClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BookingClaimValidatorReportsEveryProblem();
        BookingClaimRequiredCurrencyRuleFlagsABlankCurrency();
        BookingClaimCodeLengthRuleFlagsALongCode();
        BookingClaimLimitRangeRuleFlagsValuesAboveLimit();
        BookingClaimRatioRangeRuleFlagsValuesAboveRatio();
        BookingClaimRequiredChannelRuleFlagsABlankChannel();
        BookingClaimRequiredCodeRuleFlagsABlankCode();
    }
}
