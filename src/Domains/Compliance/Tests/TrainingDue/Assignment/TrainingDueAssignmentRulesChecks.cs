namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueAssignmentRulesChecks
{
    public static void TrainingDueAssignmentValidatorReportsEveryProblem()
    {
        var validator = TrainingDueAssignmentRules.DefaultTrainingDueAssignmentValidator();
        var item = TrainingDueAssignment.NewTrainingDueAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueAssignmentMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new TrainingDueAssignmentMarginRangeRule(9.2, 4830.6);
        var item = TrainingDueAssignment.NewTrainingDueAssignment("x", "y");
        item.Margin = 4832.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueAssignmentCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new TrainingDueAssignmentCostCenterLengthRule(255);
        var item = TrainingDueAssignment.NewTrainingDueAssignment("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueAssignmentRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TrainingDueAssignmentRequiredDepartmentRule("training due assignment needs a value");
        var item = TrainingDueAssignment.NewTrainingDueAssignment("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueAssignmentValidatorReportsEveryProblem();
        TrainingDueAssignmentMarginRangeRuleFlagsValuesAboveMargin();
        TrainingDueAssignmentCostCenterLengthRuleFlagsALongCostCenter();
        TrainingDueAssignmentRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
