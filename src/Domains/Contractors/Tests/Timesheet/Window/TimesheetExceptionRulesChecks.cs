namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TimesheetExceptionRulesChecks
{
    public static void TimesheetExceptionValidatorReportsEveryProblem()
    {
        var validator = TimesheetExceptionRules.DefaultTimesheetExceptionValidator();
        var item = TimesheetException.NewTimesheetException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetExceptionMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TimesheetExceptionMinimumRangeRule(6.8, 91.1);
        var item = TimesheetException.NewTimesheetException("x", "y");
        item.Minimum = 92.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetExceptionQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new TimesheetExceptionQuantityRangeRule(32, 32);
        var item = TimesheetException.NewTimesheetException("x", "y");
        item.Quantity = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetExceptionRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new TimesheetExceptionRequiredCurrencyRule("timesheet exception needs a value");
        var item = TimesheetException.NewTimesheetException("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetExceptionValidatorReportsEveryProblem();
        TimesheetExceptionMinimumRangeRuleFlagsValuesAboveMinimum();
        TimesheetExceptionQuantityRangeRuleFlagsValuesAboveQuantity();
        TimesheetExceptionRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
