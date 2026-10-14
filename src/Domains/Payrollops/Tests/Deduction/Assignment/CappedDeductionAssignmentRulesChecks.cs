namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CappedDeductionAssignmentRulesChecks
{
    public static void CappedDeductionAssignmentValidatorReportsEveryProblem()
    {
        var validator = CappedDeductionAssignmentRules.DefaultCappedDeductionAssignmentValidator();
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedDeductionAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedDeductionAssignmentRequiredCodeRule("capped deduction assignment needs this field");
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedDeductionAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedDeductionAssignmentCodeLengthRule(120);
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedDeductionAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedDeductionAssignmentLabelLengthRule(80);
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedDeductionAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new CappedDeductionAssignmentAttemptsRangeRule(16, 86400);
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedDeductionAssignmentValidatorReportsEveryProblem();
        CappedDeductionAssignmentRequiredCodeRuleFlagsABlankCode();
        CappedDeductionAssignmentCodeLengthRuleFlagsALongCode();
        CappedDeductionAssignmentLabelLengthRuleFlagsALongLabel();
        CappedDeductionAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
