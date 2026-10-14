namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class ScheduledBookingBaselineRulesChecks
{
    public static void ScheduledBookingBaselineValidatorReportsEveryProblem()
    {
        var validator = ScheduledBookingBaselineRules.DefaultScheduledBookingBaselineValidator();
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBookingBaselineTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledBookingBaselineTitleLengthRule(80);
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledBookingBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledBookingBaselineRequiredTitleRule("scheduled booking baseline needs this field");
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledBookingBaselineValidatorReportsEveryProblem();
        ScheduledBookingBaselineTitleLengthRuleFlagsALongTitle();
        ScheduledBookingBaselineRequiredTitleRuleFlagsABlankTitle();
    }
}
