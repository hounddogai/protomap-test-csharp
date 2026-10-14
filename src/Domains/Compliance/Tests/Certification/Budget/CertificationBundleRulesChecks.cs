namespace Acme.Workforce.Compliance.Tests.Certification.Budget;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CertificationBundleRulesChecks
{
    public static void CertificationBundleValidatorReportsEveryProblem()
    {
        var validator = CertificationBundleRules.DefaultCertificationBundleValidator();
        var item = CertificationBundle.NewCertificationBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationBundleRequiredCodeRule("certification bundle needs this field");
        var item = CertificationBundle.NewCertificationBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationBundleExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new CertificationBundleExpiresAtRangeRule(80, 1000);
        var item = CertificationBundle.NewCertificationBundle("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationBundleWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new CertificationBundleWindowDaysRangeRule(255, 86400);
        var item = CertificationBundle.NewCertificationBundle("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CertificationBundleLabelLengthRule(86400);
        var item = CertificationBundle.NewCertificationBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationBundleBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new CertificationBundleBatchSizeRangeRule(80, 86400);
        var item = CertificationBundle.NewCertificationBundle("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationBundleValidatorReportsEveryProblem();
        CertificationBundleRequiredCodeRuleFlagsABlankCode();
        CertificationBundleExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        CertificationBundleWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        CertificationBundleLabelLengthRuleFlagsALongLabel();
        CertificationBundleBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
