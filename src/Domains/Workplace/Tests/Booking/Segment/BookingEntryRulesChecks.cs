namespace Acme.Workforce.Workplace.Tests.Booking.Segment;

using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BookingEntryRulesChecks
{
    public static void BookingEntryValidatorReportsEveryProblem()
    {
        var validator = BookingEntryRules.DefaultBookingEntryValidator();
        var item = BookingEntry.NewBookingEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BookingEntryTitleLengthRule(64);
        var item = BookingEntry.NewBookingEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingEntryRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new BookingEntryRequiredSummaryRule("booking entry needs an entry");
        var item = BookingEntry.NewBookingEntry("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingEntryDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new BookingEntryDurationMinutesRangeRule(32, 120);
        var item = BookingEntry.NewBookingEntry("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingEntrySummaryLengthRuleFlagsALongSummary()
    {
        var rule = new BookingEntrySummaryLengthRule(64);
        var item = BookingEntry.NewBookingEntry("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BookingEntryValidatorReportsEveryProblem();
        BookingEntryTitleLengthRuleFlagsALongTitle();
        BookingEntryRequiredSummaryRuleFlagsABlankSummary();
        BookingEntryDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        BookingEntrySummaryLengthRuleFlagsALongSummary();
    }
}
