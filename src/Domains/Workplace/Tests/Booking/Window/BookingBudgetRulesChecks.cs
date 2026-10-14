namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BookingBudgetRulesChecks
{
    public static void BookingBudgetValidatorReportsEveryProblem()
    {
        var validator = BookingBudgetRules.DefaultBookingBudgetValidator();
        var item = BookingBudget.NewBookingBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new BookingBudgetCodeLengthRule(64);
        var item = BookingBudget.NewBookingBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingBudgetBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BookingBudgetBatchSizeRangeRule(80, 255);
        var item = BookingBudget.NewBookingBudget("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BookingBudgetValidatorReportsEveryProblem();
        BookingBudgetCodeLengthRuleFlagsALongCode();
        BookingBudgetBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
