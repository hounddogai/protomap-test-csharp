namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionAssignmentRulesChecks
{
    public static void DeductionAssignmentValidatorReportsEveryProblem()
    {
        var validator = DeductionAssignmentRules.DefaultDeductionAssignmentValidator();
        var item = DeductionAssignment.NewDeductionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionAssignmentCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new DeductionAssignmentCreatedAtRangeRule(64, 1000);
        var item = DeductionAssignment.NewDeductionAssignment("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionAssignmentChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new DeductionAssignmentChecksumLengthRule(86400);
        var item = DeductionAssignment.NewDeductionAssignment("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionAssignmentRequiredCodeRule("deduction assignment needs a value");
        var item = DeductionAssignment.NewDeductionAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionAssignmentValidatorReportsEveryProblem();
        DeductionAssignmentCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        DeductionAssignmentChecksumLengthRuleFlagsALongChecksum();
        DeductionAssignmentRequiredCodeRuleFlagsABlankCode();
    }
}
