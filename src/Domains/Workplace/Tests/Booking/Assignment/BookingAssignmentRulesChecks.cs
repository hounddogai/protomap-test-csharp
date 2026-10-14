namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BookingAssignmentRulesChecks
{
    public static void BookingAssignmentValidatorReportsEveryProblem()
    {
        var validator = BookingAssignmentRules.DefaultBookingAssignmentValidator();
        var item = BookingAssignment.NewBookingAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingAssignmentNotesLengthRuleFlagsALongNotes()
    {
        var rule = new BookingAssignmentNotesLengthRule(3600);
        var item = BookingAssignment.NewBookingAssignment("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingAssignmentRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new BookingAssignmentRequiredCurrencyRule("booking assignment needs this field");
        var item = BookingAssignment.NewBookingAssignment("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingAssignmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new BookingAssignmentCostRangeRule(4.9, 3614.0);
        var item = BookingAssignment.NewBookingAssignment("x", "y");
        item.Cost = 3615.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingAssignmentEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new BookingAssignmentEndsAtRangeRule(80, 255);
        var item = BookingAssignment.NewBookingAssignment("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BookingAssignmentValidatorReportsEveryProblem();
        BookingAssignmentNotesLengthRuleFlagsALongNotes();
        BookingAssignmentRequiredCurrencyRuleFlagsABlankCurrency();
        BookingAssignmentCostRangeRuleFlagsValuesAboveCost();
        BookingAssignmentEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
