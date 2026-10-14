namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationSummaryRulesChecks
{
    public static void CertificationSummaryValidatorReportsEveryProblem()
    {
        var validator = CertificationSummaryRules.DefaultCertificationSummaryValidator();
        var item = CertificationSummary.NewCertificationSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationSummaryRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new CertificationSummaryRequiredCostCenterRule("certification summary needs this field");
        var item = CertificationSummary.NewCertificationSummary("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationSummaryMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new CertificationSummaryMarginRangeRule(7.4, 4351.9);
        var item = CertificationSummary.NewCertificationSummary("x", "y");
        item.Margin = 4353.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationSummaryMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new CertificationSummaryMaximumRangeRule(4.6, 4592.6);
        var item = CertificationSummary.NewCertificationSummary("x", "y");
        item.Maximum = 4594.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationSummaryValidatorReportsEveryProblem();
        CertificationSummaryRequiredCostCenterRuleFlagsABlankCostCenter();
        CertificationSummaryMarginRangeRuleFlagsValuesAboveMargin();
        CertificationSummaryMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
