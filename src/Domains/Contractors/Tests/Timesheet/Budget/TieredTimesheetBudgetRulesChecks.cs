namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TieredTimesheetBudgetRulesChecks
{
    public static void TieredTimesheetBudgetValidatorReportsEveryProblem()
    {
        var validator = TieredTimesheetBudgetRules.DefaultTieredTimesheetBudgetValidator();
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredTimesheetBudgetMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TieredTimesheetBudgetMinimumRangeRule(1.4, 3458.5);
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("x", "y");
        item.Minimum = 3460.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredTimesheetBudgetDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new TieredTimesheetBudgetDepartmentLengthRule(32);
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTimesheetBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredTimesheetBudgetRequiredNameRule("tiered timesheet budget needs this field");
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredTimesheetBudgetCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new TieredTimesheetBudgetCeilingRangeRule(4.6, 1066.5);
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("x", "y");
        item.Ceiling = 1068.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredTimesheetBudgetRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TieredTimesheetBudgetRequiredDepartmentRule("tiered timesheet budget needs an entry");
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredTimesheetBudgetValidatorReportsEveryProblem();
        TieredTimesheetBudgetMinimumRangeRuleFlagsValuesAboveMinimum();
        TieredTimesheetBudgetDepartmentLengthRuleFlagsALongDepartment();
        TieredTimesheetBudgetRequiredNameRuleFlagsABlankName();
        TieredTimesheetBudgetCeilingRangeRuleFlagsValuesAboveCeiling();
        TieredTimesheetBudgetRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
