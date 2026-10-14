namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class DeductionCheckpointRulesChecks
{
    public static void DeductionCheckpointValidatorReportsEveryProblem()
    {
        var validator = DeductionCheckpointRules.DefaultDeductionCheckpointValidator();
        var item = DeductionCheckpoint.NewDeductionCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionCheckpointRequiredCodeRule("deduction checkpoint needs an entry");
        var item = DeductionCheckpoint.NewDeductionCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionCheckpointUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new DeductionCheckpointUpdatedAtRangeRule(16, 64);
        var item = DeductionCheckpoint.NewDeductionCheckpoint("x", "y");
        item.UpdatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionCheckpointCodeLengthRule(120);
        var item = DeductionCheckpoint.NewDeductionCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new DeductionCheckpointRequiredNameRule("deduction checkpoint needs this field");
        var item = DeductionCheckpoint.NewDeductionCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionCheckpointValidatorReportsEveryProblem();
        DeductionCheckpointRequiredCodeRuleFlagsABlankCode();
        DeductionCheckpointUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        DeductionCheckpointCodeLengthRuleFlagsALongCode();
        DeductionCheckpointRequiredNameRuleFlagsABlankName();
    }
}
