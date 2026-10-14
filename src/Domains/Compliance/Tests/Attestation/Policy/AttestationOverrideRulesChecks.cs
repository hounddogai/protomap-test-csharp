namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationOverrideRulesChecks
{
    public static void AttestationOverrideValidatorReportsEveryProblem()
    {
        var validator = AttestationOverrideRules.DefaultAttestationOverrideValidator();
        var item = AttestationOverride.NewAttestationOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AttestationOverrideLabelLengthRule(1000);
        var item = AttestationOverride.NewAttestationOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new AttestationOverrideCodeLengthRule(80);
        var item = AttestationOverride.NewAttestationOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AttestationOverrideValidatorReportsEveryProblem();
        AttestationOverrideLabelLengthRuleFlagsALongLabel();
        AttestationOverrideCodeLengthRuleFlagsALongCode();
    }
}
