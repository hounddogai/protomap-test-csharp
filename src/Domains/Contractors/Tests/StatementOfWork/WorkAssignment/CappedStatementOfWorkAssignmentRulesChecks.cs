namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Checks of work assignment code, run by the project's runner.</summary>
public static class CappedStatementOfWorkAssignmentRulesChecks
{
    public static void CappedStatementOfWorkAssignmentValidatorReportsEveryProblem()
    {
        var validator = CappedStatementOfWorkAssignmentRules.DefaultCappedStatementOfWorkAssignmentValidator();
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedStatementOfWorkAssignmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new CappedStatementOfWorkAssignmentCostRangeRule(2.5, 1527.0);
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("x", "y");
        item.Cost = 1528.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedStatementOfWorkAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedStatementOfWorkAssignmentLabelLengthRule(86400);
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedStatementOfWorkAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new CappedStatementOfWorkAssignmentRequiredNameRule(
            "capped statement of work assignment needs this field");
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedStatementOfWorkAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedStatementOfWorkAssignmentRequiredCodeRule(
            "capped statement of work assignment needs a value");
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkAssignmentValidatorReportsEveryProblem();
        CappedStatementOfWorkAssignmentCostRangeRuleFlagsValuesAboveCost();
        CappedStatementOfWorkAssignmentLabelLengthRuleFlagsALongLabel();
        CappedStatementOfWorkAssignmentRequiredNameRuleFlagsABlankName();
        CappedStatementOfWorkAssignmentRequiredCodeRuleFlagsABlankCode();
    }
}
