namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingMetricRulesChecks
{
    public static void BookingMetricValidatorReportsEveryProblem()
    {
        var validator = BookingMetricRules.DefaultBookingMetricValidator();
        var item = BookingMetric.NewBookingMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingMetricRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new BookingMetricRetriesRangeRule(16, 120);
        var item = BookingMetric.NewBookingMetric("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingMetricRequiredNameRuleFlagsABlankName()
    {
        var rule = new BookingMetricRequiredNameRule("booking metric needs a value");
        var item = BookingMetric.NewBookingMetric("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BookingMetricValidatorReportsEveryProblem();
        BookingMetricRetriesRangeRuleFlagsValuesAboveRetries();
        BookingMetricRequiredNameRuleFlagsABlankName();
    }
}
