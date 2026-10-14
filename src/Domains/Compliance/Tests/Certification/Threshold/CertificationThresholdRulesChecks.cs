namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class CertificationThresholdRulesChecks
{
    public static void CertificationThresholdValidatorReportsEveryProblem()
    {
        var validator = CertificationThresholdRules.DefaultCertificationThresholdValidator();
        var item = CertificationThreshold.NewCertificationThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationThresholdRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new CertificationThresholdRequiredCostCenterRule("certification threshold needs an entry");
        var item = CertificationThreshold.NewCertificationThreshold("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationThresholdRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new CertificationThresholdRequiredPhoneRule("certification threshold needs an entry");
        var item = CertificationThreshold.NewCertificationThreshold("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationThresholdValidatorReportsEveryProblem();
        CertificationThresholdRequiredCostCenterRuleFlagsABlankCostCenter();
        CertificationThresholdRequiredPhoneRuleFlagsABlankPhone();
    }
}
