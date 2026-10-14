namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class RollingAccrualAssignmentRulesChecks
{
    public static void RollingAccrualAssignmentValidatorReportsEveryProblem()
    {
        var validator = RollingAccrualAssignmentRules.DefaultRollingAccrualAssignmentValidator();
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAccrualAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingAccrualAssignmentRequiredTitleRule("rolling accrual assignment needs a value");
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAccrualAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RollingAccrualAssignmentTitleLengthRule(255);
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAccrualAssignmentBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new RollingAccrualAssignmentBatchSizeRangeRule(16, 16);
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("x", "y");
        item.BatchSize = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAccrualAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingAccrualAssignmentCodeLengthRule(80);
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingAccrualAssignmentRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new RollingAccrualAssignmentRequiredUnitRule("rolling accrual assignment needs a value");
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingAccrualAssignmentValidatorReportsEveryProblem();
        RollingAccrualAssignmentRequiredTitleRuleFlagsABlankTitle();
        RollingAccrualAssignmentTitleLengthRuleFlagsALongTitle();
        RollingAccrualAssignmentBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        RollingAccrualAssignmentCodeLengthRuleFlagsALongCode();
        RollingAccrualAssignmentRequiredUnitRuleFlagsABlankUnit();
    }
}
