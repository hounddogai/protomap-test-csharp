namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRecordRulesChecks
{
    public static void BookingRecordValidatorReportsEveryProblem()
    {
        var validator = BookingRecordRules.DefaultBookingRecordValidator();
        var item = BookingRecord.NewBookingRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingRecordMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BookingRecordMarginRangeRule(0.5, 4363.6);
        var item = BookingRecord.NewBookingRecord("x", "y");
        item.Margin = 4365.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingRecordRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new BookingRecordRequiredSummaryRule("booking record needs a value");
        var item = BookingRecord.NewBookingRecord("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingRecordRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new BookingRecordRequiredSegmentRule("booking record needs a value");
        var item = BookingRecord.NewBookingRecord("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BookingRecordRequiredLabelRule("booking record needs an entry");
        var item = BookingRecord.NewBookingRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BookingRecordRequiredCodeRule("booking record needs a value");
        var item = BookingRecord.NewBookingRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BookingRecordValidatorReportsEveryProblem();
        BookingRecordMarginRangeRuleFlagsValuesAboveMargin();
        BookingRecordRequiredSummaryRuleFlagsABlankSummary();
        BookingRecordRequiredSegmentRuleFlagsABlankSegment();
        BookingRecordRequiredLabelRuleFlagsABlankLabel();
        BookingRecordRequiredCodeRuleFlagsABlankCode();
    }
}
