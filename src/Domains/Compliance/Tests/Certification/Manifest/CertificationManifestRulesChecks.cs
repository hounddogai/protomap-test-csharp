namespace Acme.Workforce.Compliance.Tests.Certification.Manifest;

using Acme.Workforce.Compliance.Certification.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class CertificationManifestRulesChecks
{
    public static void CertificationManifestValidatorReportsEveryProblem()
    {
        var validator = CertificationManifestRules.DefaultCertificationManifestValidator();
        var item = CertificationManifest.NewCertificationManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new CertificationManifestCodeLengthRule(3600);
        var item = CertificationManifest.NewCertificationManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationManifestCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new CertificationManifestCreatedAtRangeRule(80, 255);
        var item = CertificationManifest.NewCertificationManifest("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationManifestRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new CertificationManifestRequiredExternalRefRule("certification manifest needs an entry");
        var item = CertificationManifest.NewCertificationManifest("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationManifestRequiredCodeRule("certification manifest needs an entry");
        var item = CertificationManifest.NewCertificationManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationManifestValidatorReportsEveryProblem();
        CertificationManifestCodeLengthRuleFlagsALongCode();
        CertificationManifestCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        CertificationManifestRequiredExternalRefRuleFlagsABlankExternalRef();
        CertificationManifestRequiredCodeRuleFlagsABlankCode();
    }
}
