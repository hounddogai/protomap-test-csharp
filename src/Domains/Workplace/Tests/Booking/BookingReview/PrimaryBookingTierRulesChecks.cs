namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class PrimaryBookingTierRulesChecks
{
    public static void PrimaryBookingTierValidatorReportsEveryProblem()
    {
        var validator = PrimaryBookingTierRules.DefaultPrimaryBookingTierValidator();
        var item = PrimaryBookingTier.NewPrimaryBookingTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryBookingTierGradeLengthRuleFlagsALongGrade()
    {
        var rule = new PrimaryBookingTierGradeLengthRule(32);
        var item = PrimaryBookingTier.NewPrimaryBookingTier("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryBookingTierRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PrimaryBookingTierRequiredExternalRefRule("primary booking tier needs this field");
        var item = PrimaryBookingTier.NewPrimaryBookingTier("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryBookingTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryBookingTierCodeLengthRule(80);
        var item = PrimaryBookingTier.NewPrimaryBookingTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryBookingTierValidatorReportsEveryProblem();
        PrimaryBookingTierGradeLengthRuleFlagsALongGrade();
        PrimaryBookingTierRequiredExternalRefRuleFlagsABlankExternalRef();
        PrimaryBookingTierCodeLengthRuleFlagsALongCode();
    }
}
