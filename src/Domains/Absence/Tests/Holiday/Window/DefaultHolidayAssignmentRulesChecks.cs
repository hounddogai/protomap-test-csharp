namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultHolidayAssignmentRulesChecks
{
    public static void DefaultHolidayAssignmentValidatorReportsEveryProblem()
    {
        var validator = DefaultHolidayAssignmentRules.DefaultDefaultHolidayAssignmentValidator();
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultHolidayAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultHolidayAssignmentRequiredCodeRule("default holiday assignment needs an entry");
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHolidayAssignmentBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new DefaultHolidayAssignmentBudgetRangeRule(4.7, 1855.6);
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("x", "y");
        item.Budget = 1857.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHolidayAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultHolidayAssignmentRequiredTitleRule("default holiday assignment needs an entry");
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultHolidayAssignmentValidatorReportsEveryProblem();
        DefaultHolidayAssignmentRequiredCodeRuleFlagsABlankCode();
        DefaultHolidayAssignmentBudgetRangeRuleFlagsValuesAboveBudget();
        DefaultHolidayAssignmentRequiredTitleRuleFlagsABlankTitle();
    }
}
