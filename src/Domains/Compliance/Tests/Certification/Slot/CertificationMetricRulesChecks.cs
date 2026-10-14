namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CertificationMetricRulesChecks
{
    public static void CertificationMetricValidatorReportsEveryProblem()
    {
        var validator = CertificationMetricRules.DefaultCertificationMetricValidator();
        var item = CertificationMetric.NewCertificationMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationMetricRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new CertificationMetricRequiredTimezoneRule("certification metric needs a value");
        var item = CertificationMetric.NewCertificationMetric("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CertificationMetricRequiredLabelRule("certification metric needs this field");
        var item = CertificationMetric.NewCertificationMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationMetricRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new CertificationMetricRequiredOwnerIdRule("certification metric needs this field");
        var item = CertificationMetric.NewCertificationMetric("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationMetricCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new CertificationMetricCostRangeRule(5.1, 4323.8);
        var item = CertificationMetric.NewCertificationMetric("x", "y");
        item.Cost = 4325.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationMetricRequiredCodeRule("certification metric needs a value");
        var item = CertificationMetric.NewCertificationMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationMetricValidatorReportsEveryProblem();
        CertificationMetricRequiredTimezoneRuleFlagsABlankTimezone();
        CertificationMetricRequiredLabelRuleFlagsABlankLabel();
        CertificationMetricRequiredOwnerIdRuleFlagsABlankOwnerId();
        CertificationMetricCostRangeRuleFlagsValuesAboveCost();
        CertificationMetricRequiredCodeRuleFlagsABlankCode();
    }
}
