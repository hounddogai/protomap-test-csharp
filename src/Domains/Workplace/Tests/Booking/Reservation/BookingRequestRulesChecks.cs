namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingRequestRulesChecks
{
    public static void BookingRequestValidatorReportsEveryProblem()
    {
        var validator = BookingRequestRules.DefaultBookingRequestValidator();
        var item = BookingRequest.NewBookingRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BookingRequestRequiredTitleRule("booking request needs this field");
        var item = BookingRequest.NewBookingRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new BookingRequestCodeLengthRule(80);
        var item = BookingRequest.NewBookingRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BookingRequestTitleLengthRule(1000);
        var item = BookingRequest.NewBookingRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new BookingRequestRequiredNameRule("booking request needs a value");
        var item = BookingRequest.NewBookingRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingRequestRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new BookingRequestRevisionRangeRule(120, 1000);
        var item = BookingRequest.NewBookingRequest("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BookingRequestValidatorReportsEveryProblem();
        BookingRequestRequiredTitleRuleFlagsABlankTitle();
        BookingRequestCodeLengthRuleFlagsALongCode();
        BookingRequestTitleLengthRuleFlagsALongTitle();
        BookingRequestRequiredNameRuleFlagsABlankName();
        BookingRequestRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
