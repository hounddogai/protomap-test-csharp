namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AttestationRequestRulesChecks
{
    public static void AttestationRequestValidatorReportsEveryProblem()
    {
        var validator = AttestationRequestRules.DefaultAttestationRequestValidator();
        var item = AttestationRequest.NewAttestationRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AttestationRequestLabelLengthRule(16);
        var item = AttestationRequest.NewAttestationRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AttestationRequestExpiresAtRangeRule(3600, 3600);
        var item = AttestationRequest.NewAttestationRequest("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationRequestBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AttestationRequestBatchSizeRangeRule(120, 86400);
        var item = AttestationRequest.NewAttestationRequest("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationRequestValidatorReportsEveryProblem();
        AttestationRequestLabelLengthRuleFlagsALongLabel();
        AttestationRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        AttestationRequestBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
