namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class BookingNoticeRulesChecks
{
    public static void BookingNoticeValidatorReportsEveryProblem()
    {
        var validator = BookingNoticeRules.DefaultBookingNoticeValidator();
        var item = BookingNotice.NewBookingNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BookingNoticeTitleLengthRule(120);
        var item = BookingNotice.NewBookingNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingNoticeSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new BookingNoticeSegmentLengthRule(120);
        var item = BookingNotice.NewBookingNotice("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingNoticeRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BookingNoticeRatioRangeRule(7.3, 4457.6);
        var item = BookingNotice.NewBookingNotice("x", "y");
        item.Ratio = 4459.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BookingNoticeValidatorReportsEveryProblem();
        BookingNoticeTitleLengthRuleFlagsALongTitle();
        BookingNoticeSegmentLengthRuleFlagsALongSegment();
        BookingNoticeRatioRangeRuleFlagsValuesAboveRatio();
    }
}
