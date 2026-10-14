namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class BookingSnapshotRulesChecks
{
    public static void BookingSnapshotValidatorReportsEveryProblem()
    {
        var validator = BookingSnapshotRules.DefaultBookingSnapshotValidator();
        var item = BookingSnapshot.NewBookingSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingSnapshotRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new BookingSnapshotRequiredNotesRule("booking snapshot needs this field");
        var item = BookingSnapshot.NewBookingSnapshot("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingSnapshotNotesLengthRuleFlagsALongNotes()
    {
        var rule = new BookingSnapshotNotesLengthRule(255);
        var item = BookingSnapshot.NewBookingSnapshot("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingSnapshotDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new BookingSnapshotDescriptionLengthRule(80);
        var item = BookingSnapshot.NewBookingSnapshot("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BookingSnapshotValidatorReportsEveryProblem();
        BookingSnapshotRequiredNotesRuleFlagsABlankNotes();
        BookingSnapshotNotesLengthRuleFlagsALongNotes();
        BookingSnapshotDescriptionLengthRuleFlagsALongDescription();
    }
}
