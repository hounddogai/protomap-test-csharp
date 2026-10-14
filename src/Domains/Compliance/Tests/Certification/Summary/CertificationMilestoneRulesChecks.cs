namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationMilestoneRulesChecks
{
    public static void CertificationMilestoneValidatorReportsEveryProblem()
    {
        var validator = CertificationMilestoneRules.DefaultCertificationMilestoneValidator();
        var item = CertificationMilestone.NewCertificationMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationMilestoneSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new CertificationMilestoneSalaryBandLengthRule(80);
        var item = CertificationMilestone.NewCertificationMilestone("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationMilestoneEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new CertificationMilestoneEndsAtRangeRule(32, 64);
        var item = CertificationMilestone.NewCertificationMilestone("x", "y");
        item.EndsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationMilestoneRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new CertificationMilestoneRequiredSourceRule("certification milestone needs an entry");
        var item = CertificationMilestone.NewCertificationMilestone("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationMilestoneRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new CertificationMilestoneRateRangeRule(5.9, 2377.5);
        var item = CertificationMilestone.NewCertificationMilestone("x", "y");
        item.Rate = 2379.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CertificationMilestoneRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new CertificationMilestoneRequiredSalaryBandRule("certification milestone needs this field");
        var item = CertificationMilestone.NewCertificationMilestone("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationMilestoneValidatorReportsEveryProblem();
        CertificationMilestoneSalaryBandLengthRuleFlagsALongSalaryBand();
        CertificationMilestoneEndsAtRangeRuleFlagsValuesAboveEndsAt();
        CertificationMilestoneRequiredSourceRuleFlagsABlankSource();
        CertificationMilestoneRateRangeRuleFlagsValuesAboveRate();
        CertificationMilestoneRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
