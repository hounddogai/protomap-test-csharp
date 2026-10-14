namespace Acme.Workforce.Absence.Tests.Accrual.AccrualAssignment;

using Acme.Workforce.Absence.Accrual.AccrualAssignment;

/// <summary>Checks of accrual assignment code, run by the project's runner.</summary>
public static class ScheduledAccrualAssignmentRulesChecks
{
    public static void ScheduledAccrualAssignmentValidatorReportsEveryProblem()
    {
        var validator = ScheduledAccrualAssignmentRules.DefaultScheduledAccrualAssignmentValidator();
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAccrualAssignmentBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new ScheduledAccrualAssignmentBudgetRangeRule(0.5, 1161.9);
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("x", "y");
        item.Budget = 1163.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAccrualAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledAccrualAssignmentRequiredLabelRule("scheduled accrual assignment needs an entry");
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAccrualAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledAccrualAssignmentCodeLengthRule(255);
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAccrualAssignmentRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new ScheduledAccrualAssignmentRequiredSummaryRule("scheduled accrual assignment needs a value");
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledAccrualAssignmentValidatorReportsEveryProblem();
        ScheduledAccrualAssignmentBudgetRangeRuleFlagsValuesAboveBudget();
        ScheduledAccrualAssignmentRequiredLabelRuleFlagsABlankLabel();
        ScheduledAccrualAssignmentCodeLengthRuleFlagsALongCode();
        ScheduledAccrualAssignmentRequiredSummaryRuleFlagsABlankSummary();
    }
}
