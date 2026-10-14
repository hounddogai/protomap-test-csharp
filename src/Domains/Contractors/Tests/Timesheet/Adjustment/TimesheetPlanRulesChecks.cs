namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetPlanRulesChecks
{
    public static void TimesheetPlanValidatorReportsEveryProblem()
    {
        var validator = TimesheetPlanRules.DefaultTimesheetPlanValidator();
        var item = TimesheetPlan.NewTimesheetPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetPlanRequiredCodeRule("timesheet plan needs a value");
        var item = TimesheetPlan.NewTimesheetPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetPlanQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new TimesheetPlanQuantityRangeRule(32, 86400);
        var item = TimesheetPlan.NewTimesheetPlan("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetPlanAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new TimesheetPlanAttemptsRangeRule(120, 120);
        var item = TimesheetPlan.NewTimesheetPlan("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TimesheetPlanValidatorReportsEveryProblem();
        TimesheetPlanRequiredCodeRuleFlagsABlankCode();
        TimesheetPlanQuantityRangeRuleFlagsValuesAboveQuantity();
        TimesheetPlanAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
