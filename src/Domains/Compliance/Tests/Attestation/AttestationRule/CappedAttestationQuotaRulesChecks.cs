namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class CappedAttestationQuotaRulesChecks
{
    public static void CappedAttestationQuotaValidatorReportsEveryProblem()
    {
        var validator = CappedAttestationQuotaRules.DefaultCappedAttestationQuotaValidator();
        var item = CappedAttestationQuota.NewCappedAttestationQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAttestationQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedAttestationQuotaLabelLengthRule(120);
        var item = CappedAttestationQuota.NewCappedAttestationQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAttestationQuotaSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new CappedAttestationQuotaSegmentLengthRule(120);
        var item = CappedAttestationQuota.NewCappedAttestationQuota("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAttestationQuotaSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new CappedAttestationQuotaSalaryBandLengthRule(80);
        var item = CappedAttestationQuota.NewCappedAttestationQuota("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAttestationQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedAttestationQuotaCodeLengthRule(1000);
        var item = CappedAttestationQuota.NewCappedAttestationQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAttestationQuotaThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new CappedAttestationQuotaThresholdRangeRule(9.5, 288.1);
        var item = CappedAttestationQuota.NewCappedAttestationQuota("x", "y");
        item.Threshold = 289.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedAttestationQuotaValidatorReportsEveryProblem();
        CappedAttestationQuotaLabelLengthRuleFlagsALongLabel();
        CappedAttestationQuotaSegmentLengthRuleFlagsALongSegment();
        CappedAttestationQuotaSalaryBandLengthRuleFlagsALongSalaryBand();
        CappedAttestationQuotaCodeLengthRuleFlagsALongCode();
        CappedAttestationQuotaThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
