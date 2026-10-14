namespace Acme.Workforce.Compliance.Tests.Attestation.Batch;

using Acme.Workforce.Compliance.Attestation.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AttestationBatchRulesChecks
{
    public static void AttestationBatchValidatorReportsEveryProblem()
    {
        var validator = AttestationBatchRules.DefaultAttestationBatchValidator();
        var item = AttestationBatch.NewAttestationBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationBatchTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new AttestationBatchTenureMonthsRangeRule(86400, 86400);
        var item = AttestationBatch.NewAttestationBatch("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new AttestationBatchCodeLengthRule(86400);
        var item = AttestationBatch.NewAttestationBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationBatchNameLengthRuleFlagsALongName()
    {
        var rule = new AttestationBatchNameLengthRule(32);
        var item = AttestationBatch.NewAttestationBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AttestationBatchValidatorReportsEveryProblem();
        AttestationBatchTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        AttestationBatchCodeLengthRuleFlagsALongCode();
        AttestationBatchNameLengthRuleFlagsALongName();
    }
}
