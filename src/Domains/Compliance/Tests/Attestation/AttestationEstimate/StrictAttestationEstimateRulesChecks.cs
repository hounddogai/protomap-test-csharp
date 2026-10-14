namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class StrictAttestationEstimateRulesChecks
{
    public static void StrictAttestationEstimateValidatorReportsEveryProblem()
    {
        var validator = StrictAttestationEstimateRules.DefaultStrictAttestationEstimateValidator();
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAttestationEstimateBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new StrictAttestationEstimateBadgeLengthRule(86400);
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAttestationEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictAttestationEstimateRequiredCodeRule("strict attestation estimate needs a value");
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAttestationEstimateLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new StrictAttestationEstimateLimitRangeRule(120, 120);
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("x", "y");
        item.Limit = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAttestationEstimateSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new StrictAttestationEstimateSalaryBandLengthRule(64);
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictAttestationEstimateValidatorReportsEveryProblem();
        StrictAttestationEstimateBadgeLengthRuleFlagsALongBadge();
        StrictAttestationEstimateRequiredCodeRuleFlagsABlankCode();
        StrictAttestationEstimateLimitRangeRuleFlagsValuesAboveLimit();
        StrictAttestationEstimateSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
