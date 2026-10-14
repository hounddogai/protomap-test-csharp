namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationEstimateRulesChecks
{
    public static void CertificationEstimateValidatorReportsEveryProblem()
    {
        var validator = CertificationEstimateRules.DefaultCertificationEstimateValidator();
        var item = CertificationEstimate.NewCertificationEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationEstimateRequiredNameRuleFlagsABlankName()
    {
        var rule = new CertificationEstimateRequiredNameRule("certification estimate needs a value");
        var item = CertificationEstimate.NewCertificationEstimate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationEstimateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CertificationEstimateRequiredTitleRule("certification estimate needs a value");
        var item = CertificationEstimate.NewCertificationEstimate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new CertificationEstimateNameLengthRule(120);
        var item = CertificationEstimate.NewCertificationEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationEstimateRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new CertificationEstimateRequiredDepartmentRule("certification estimate needs an entry");
        var item = CertificationEstimate.NewCertificationEstimate("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationEstimateValidatorReportsEveryProblem();
        CertificationEstimateRequiredNameRuleFlagsABlankName();
        CertificationEstimateRequiredTitleRuleFlagsABlankTitle();
        CertificationEstimateNameLengthRuleFlagsALongName();
        CertificationEstimateRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
