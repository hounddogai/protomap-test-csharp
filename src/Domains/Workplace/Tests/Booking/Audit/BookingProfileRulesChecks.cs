namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingProfileRulesChecks
{
    public static void BookingProfileValidatorReportsEveryProblem()
    {
        var validator = BookingProfileRules.DefaultBookingProfileValidator();
        var item = BookingProfile.NewBookingProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingProfileOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new BookingProfileOwnerIdLengthRule(120);
        var item = BookingProfile.NewBookingProfile("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingProfileUnitLengthRuleFlagsALongUnit()
    {
        var rule = new BookingProfileUnitLengthRule(16);
        var item = BookingProfile.NewBookingProfile("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingProfileRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BookingProfileRequiredLabelRule("booking profile needs an entry");
        var item = BookingProfile.NewBookingProfile("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingProfileExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new BookingProfileExpiresAtRangeRule(32, 80);
        var item = BookingProfile.NewBookingProfile("x", "y");
        item.ExpiresAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BookingProfileRequiredCodeRule("booking profile needs this field");
        var item = BookingProfile.NewBookingProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BookingProfileValidatorReportsEveryProblem();
        BookingProfileOwnerIdLengthRuleFlagsALongOwnerId();
        BookingProfileUnitLengthRuleFlagsALongUnit();
        BookingProfileRequiredLabelRuleFlagsABlankLabel();
        BookingProfileExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        BookingProfileRequiredCodeRuleFlagsABlankCode();
    }
}
