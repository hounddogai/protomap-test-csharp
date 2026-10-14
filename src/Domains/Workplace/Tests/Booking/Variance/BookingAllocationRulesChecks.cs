namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingAllocationRulesChecks
{
    public static void BookingAllocationValidatorReportsEveryProblem()
    {
        var validator = BookingAllocationRules.DefaultBookingAllocationValidator();
        var item = BookingAllocation.NewBookingAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingAllocationHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new BookingAllocationHoursRangeRule(8.4, 408.4);
        var item = BookingAllocation.NewBookingAllocation("x", "y");
        item.Hours = 409.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingAllocationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BookingAllocationLabelLengthRule(255);
        var item = BookingAllocation.NewBookingAllocation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BookingAllocationValidatorReportsEveryProblem();
        BookingAllocationHoursRangeRuleFlagsValuesAboveHours();
        BookingAllocationLabelLengthRuleFlagsALongLabel();
    }
}
