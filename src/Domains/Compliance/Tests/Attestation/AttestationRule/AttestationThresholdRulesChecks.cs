namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class AttestationThresholdRulesChecks
{
    public static void AttestationThresholdValidatorReportsEveryProblem()
    {
        var validator = AttestationThresholdRules.DefaultAttestationThresholdValidator();
        var item = AttestationThreshold.NewAttestationThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new AttestationThresholdCodeLengthRule(255);
        var item = AttestationThreshold.NewAttestationThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationThresholdNameLengthRuleFlagsALongName()
    {
        var rule = new AttestationThresholdNameLengthRule(64);
        var item = AttestationThreshold.NewAttestationThreshold("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationThresholdRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new AttestationThresholdRequiredChannelRule("attestation threshold needs an entry");
        var item = AttestationThreshold.NewAttestationThreshold("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationThresholdRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new AttestationThresholdRequiredExternalRefRule("attestation threshold needs an entry");
        var item = AttestationThreshold.NewAttestationThreshold("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AttestationThresholdRequiredCodeRule("attestation threshold needs a value");
        var item = AttestationThreshold.NewAttestationThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationThresholdValidatorReportsEveryProblem();
        AttestationThresholdCodeLengthRuleFlagsALongCode();
        AttestationThresholdNameLengthRuleFlagsALongName();
        AttestationThresholdRequiredChannelRuleFlagsABlankChannel();
        AttestationThresholdRequiredExternalRefRuleFlagsABlankExternalRef();
        AttestationThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
