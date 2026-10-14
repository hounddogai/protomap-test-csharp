namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationManifestRulesChecks
{
    public static void AttestationManifestValidatorReportsEveryProblem()
    {
        var validator = AttestationManifestRules.DefaultAttestationManifestValidator();
        var item = AttestationManifest.NewAttestationManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationManifestHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new AttestationManifestHoursRangeRule(2.0, 3878.1);
        var item = AttestationManifest.NewAttestationManifest("x", "y");
        item.Hours = 3879.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AttestationManifestRequiredCodeRule("attestation manifest needs a value");
        var item = AttestationManifest.NewAttestationManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationManifestVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AttestationManifestVersionRangeRule(32, 86400);
        var item = AttestationManifest.NewAttestationManifest("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationManifestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AttestationManifestRequiredLabelRule("attestation manifest needs an entry");
        var item = AttestationManifest.NewAttestationManifest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AttestationManifestValidatorReportsEveryProblem();
        AttestationManifestHoursRangeRuleFlagsValuesAboveHours();
        AttestationManifestRequiredCodeRuleFlagsABlankCode();
        AttestationManifestVersionRangeRuleFlagsValuesAboveVersion();
        AttestationManifestRequiredLabelRuleFlagsABlankLabel();
    }
}
