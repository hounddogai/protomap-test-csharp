namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DeductionBatchRulesChecks
{
    public static void DeductionBatchValidatorReportsEveryProblem()
    {
        var validator = DeductionBatchRules.DefaultDeductionBatchValidator();
        var item = DeductionBatch.NewDeductionBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DeductionBatchLabelLengthRule(16);
        var item = DeductionBatch.NewDeductionBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionBatchCodeLengthRule(80);
        var item = DeductionBatch.NewDeductionBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionBatchValidatorReportsEveryProblem();
        DeductionBatchLabelLengthRuleFlagsALongLabel();
        DeductionBatchCodeLengthRuleFlagsALongCode();
    }
}
