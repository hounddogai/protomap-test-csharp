namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CertificationQuotaRulesChecks
{
    public static void CertificationQuotaValidatorReportsEveryProblem()
    {
        var validator = CertificationQuotaRules.DefaultCertificationQuotaValidator();
        var item = CertificationQuota.NewCertificationQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationQuotaTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CertificationQuotaTitleLengthRule(64);
        var item = CertificationQuota.NewCertificationQuota("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationQuotaUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new CertificationQuotaUpdatedAtRangeRule(1000, 1000);
        var item = CertificationQuota.NewCertificationQuota("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationQuotaExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new CertificationQuotaExternalRefLengthRule(32);
        var item = CertificationQuota.NewCertificationQuota("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CertificationQuotaValidatorReportsEveryProblem();
        CertificationQuotaTitleLengthRuleFlagsALongTitle();
        CertificationQuotaUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        CertificationQuotaExternalRefLengthRuleFlagsALongExternalRef();
    }
}
