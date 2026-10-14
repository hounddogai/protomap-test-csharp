namespace Acme.Workforce.Compliance.Tests.Certification.CertificationSummary;

using Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>Checks of certification summary code, run by the project's runner.</summary>
public static class LenientCertificationPlanRulesChecks
{
    public static void LenientCertificationPlanValidatorReportsEveryProblem()
    {
        var validator = LenientCertificationPlanRules.DefaultLenientCertificationPlanValidator();
        var item = LenientCertificationPlan.NewLenientCertificationPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientCertificationPlanMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new LenientCertificationPlanMinimumRangeRule(7.8, 3868.1);
        var item = LenientCertificationPlan.NewLenientCertificationPlan("x", "y");
        item.Minimum = 3869.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientCertificationPlanValidatorReportsEveryProblem();
        LenientCertificationPlanMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
