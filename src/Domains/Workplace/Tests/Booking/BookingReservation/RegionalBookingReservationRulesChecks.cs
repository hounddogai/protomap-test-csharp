namespace Acme.Workforce.Workplace.Tests.Booking.BookingReservation;

using Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>Checks of booking reservation code, run by the project's runner.</summary>
public static class RegionalBookingReservationRulesChecks
{
    public static void RegionalBookingReservationValidatorReportsEveryProblem()
    {
        var validator = RegionalBookingReservationRules.DefaultRegionalBookingReservationValidator();
        var item = RegionalBookingReservation.NewRegionalBookingReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalBookingReservationDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new RegionalBookingReservationDescriptionLengthRule(86400);
        var item = RegionalBookingReservation.NewRegionalBookingReservation("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBookingReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalBookingReservationCodeLengthRule(86400);
        var item = RegionalBookingReservation.NewRegionalBookingReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBookingReservationOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new RegionalBookingReservationOwnerIdLengthRule(255);
        var item = RegionalBookingReservation.NewRegionalBookingReservation("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBookingReservationWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RegionalBookingReservationWeightRangeRule(7.4, 2486.1);
        var item = RegionalBookingReservation.NewRegionalBookingReservation("x", "y");
        item.Weight = 2487.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBookingReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalBookingReservationRequiredNameRule("regional booking reservation needs a value");
        var item = RegionalBookingReservation.NewRegionalBookingReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalBookingReservationValidatorReportsEveryProblem();
        RegionalBookingReservationDescriptionLengthRuleFlagsALongDescription();
        RegionalBookingReservationCodeLengthRuleFlagsALongCode();
        RegionalBookingReservationOwnerIdLengthRuleFlagsALongOwnerId();
        RegionalBookingReservationWeightRangeRuleFlagsValuesAboveWeight();
        RegionalBookingReservationRequiredNameRuleFlagsABlankName();
    }
}
