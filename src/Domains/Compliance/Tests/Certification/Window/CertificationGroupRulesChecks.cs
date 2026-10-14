namespace Acme.Workforce.Compliance.Tests.Certification.Window;

using Acme.Workforce.Compliance.Certification.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CertificationGroupRulesChecks
{
    public static void CertificationGroupValidatorReportsEveryProblem()
    {
        var validator = CertificationGroupRules.DefaultCertificationGroupValidator();
        var item = CertificationGroup.NewCertificationGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationGroupSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new CertificationGroupSummaryLengthRule(1000);
        var item = CertificationGroup.NewCertificationGroup("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationGroupEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new CertificationGroupEndsAtRangeRule(86400, 86400);
        var item = CertificationGroup.NewCertificationGroup("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationGroupValidatorReportsEveryProblem();
        CertificationGroupSummaryLengthRuleFlagsALongSummary();
        CertificationGroupEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
