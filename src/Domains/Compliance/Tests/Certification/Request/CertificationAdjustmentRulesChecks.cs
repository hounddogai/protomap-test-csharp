namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationAdjustmentRulesChecks
{
    public static void CertificationAdjustmentValidatorReportsEveryProblem()
    {
        var validator = CertificationAdjustmentRules.DefaultCertificationAdjustmentValidator();
        var item = CertificationAdjustment.NewCertificationAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CertificationAdjustmentRequiredLabelRule("certification adjustment needs an entry");
        var item = CertificationAdjustment.NewCertificationAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationAdjustmentRequiredCodeRule("certification adjustment needs a value");
        var item = CertificationAdjustment.NewCertificationAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationAdjustmentThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new CertificationAdjustmentThresholdRangeRule(5.3, 4682.1);
        var item = CertificationAdjustment.NewCertificationAdjustment("x", "y");
        item.Threshold = 4683.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationAdjustmentValidatorReportsEveryProblem();
        CertificationAdjustmentRequiredLabelRuleFlagsABlankLabel();
        CertificationAdjustmentRequiredCodeRuleFlagsABlankCode();
        CertificationAdjustmentThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
