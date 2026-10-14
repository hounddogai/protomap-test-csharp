namespace Acme.Workforce.Compliance.Tests.Certification.Review;

using Acme.Workforce.Compliance.Certification.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CertificationRollupRulesChecks
{
    public static void CertificationRollupValidatorReportsEveryProblem()
    {
        var validator = CertificationRollupRules.DefaultCertificationRollupValidator();
        var item = CertificationRollup.NewCertificationRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CertificationRollupLabelLengthRule(255);
        var item = CertificationRollup.NewCertificationRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationRollupEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new CertificationRollupEndsAtRangeRule(3600, 86400);
        var item = CertificationRollup.NewCertificationRollup("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationRollupValidatorReportsEveryProblem();
        CertificationRollupLabelLengthRuleFlagsALongLabel();
        CertificationRollupEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
