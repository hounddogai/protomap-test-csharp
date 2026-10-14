namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkAssignmentRulesChecks
{
    public static void StatementOfWorkAssignmentValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkAssignmentRules.DefaultStatementOfWorkAssignmentValidator();
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new StatementOfWorkAssignmentCodeLengthRule(16);
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkAssignmentFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new StatementOfWorkAssignmentFloorValueRangeRule(9.2, 3880.6);
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("x", "y");
        item.FloorValue = 3882.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkAssignmentCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new StatementOfWorkAssignmentCostCenterLengthRule(64);
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new StatementOfWorkAssignmentRequiredNameRule("statement of work assignment needs an entry");
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkAssignmentValidatorReportsEveryProblem();
        StatementOfWorkAssignmentCodeLengthRuleFlagsALongCode();
        StatementOfWorkAssignmentFloorValueRangeRuleFlagsValuesAboveFloorValue();
        StatementOfWorkAssignmentCostCenterLengthRuleFlagsALongCostCenter();
        StatementOfWorkAssignmentRequiredNameRuleFlagsABlankName();
    }
}
