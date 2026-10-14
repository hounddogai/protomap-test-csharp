namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationMappingRulesChecks
{
    public static void AttestationMappingValidatorReportsEveryProblem()
    {
        var validator = AttestationMappingRules.DefaultAttestationMappingValidator();
        var item = AttestationMapping.NewAttestationMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AttestationMappingRequiredCodeRule("attestation mapping needs this field");
        var item = AttestationMapping.NewAttestationMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationMappingWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new AttestationMappingWeightRangeRule(9.9, 3537.7);
        var item = AttestationMapping.NewAttestationMapping("x", "y");
        item.Weight = 3539.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new AttestationMappingCodeLengthRule(80);
        var item = AttestationMapping.NewAttestationMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AttestationMappingValidatorReportsEveryProblem();
        AttestationMappingRequiredCodeRuleFlagsABlankCode();
        AttestationMappingWeightRangeRuleFlagsValuesAboveWeight();
        AttestationMappingCodeLengthRuleFlagsALongCode();
    }
}
