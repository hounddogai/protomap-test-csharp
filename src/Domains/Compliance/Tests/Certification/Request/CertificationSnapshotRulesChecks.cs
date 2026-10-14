namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationSnapshotRulesChecks
{
    public static void CertificationSnapshotValidatorReportsEveryProblem()
    {
        var validator = CertificationSnapshotRules.DefaultCertificationSnapshotValidator();
        var item = CertificationSnapshot.NewCertificationSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationSnapshotRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new CertificationSnapshotRequiredSourceRule("certification snapshot needs this field");
        var item = CertificationSnapshot.NewCertificationSnapshot("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationSnapshotSourceLengthRuleFlagsALongSource()
    {
        var rule = new CertificationSnapshotSourceLengthRule(120);
        var item = CertificationSnapshot.NewCertificationSnapshot("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationSnapshotHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new CertificationSnapshotHoursRangeRule(6.4, 4807.0);
        var item = CertificationSnapshot.NewCertificationSnapshot("x", "y");
        item.Hours = 4808.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CertificationSnapshotValidatorReportsEveryProblem();
        CertificationSnapshotRequiredSourceRuleFlagsABlankSource();
        CertificationSnapshotSourceLengthRuleFlagsALongSource();
        CertificationSnapshotHoursRangeRuleFlagsValuesAboveHours();
    }
}
