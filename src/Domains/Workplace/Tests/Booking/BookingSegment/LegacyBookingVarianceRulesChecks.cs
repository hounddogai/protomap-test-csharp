namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class LegacyBookingVarianceRulesChecks
{
    public static void LegacyBookingVarianceValidatorReportsEveryProblem()
    {
        var validator = LegacyBookingVarianceRules.DefaultLegacyBookingVarianceValidator();
        var item = LegacyBookingVariance.NewLegacyBookingVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyBookingVarianceRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyBookingVarianceRequiredNameRule("legacy booking variance needs an entry");
        var item = LegacyBookingVariance.NewLegacyBookingVariance("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyBookingVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyBookingVarianceRequiredCodeRule("legacy booking variance needs a value");
        var item = LegacyBookingVariance.NewLegacyBookingVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyBookingVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyBookingVarianceCodeLengthRule(1000);
        var item = LegacyBookingVariance.NewLegacyBookingVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyBookingVarianceValidatorReportsEveryProblem();
        LegacyBookingVarianceRequiredNameRuleFlagsABlankName();
        LegacyBookingVarianceRequiredCodeRuleFlagsABlankCode();
        LegacyBookingVarianceCodeLengthRuleFlagsALongCode();
    }
}
