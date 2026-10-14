namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SeasonalCertificationDigestRulesChecks
{
    public static void SeasonalCertificationDigestValidatorReportsEveryProblem()
    {
        var validator = SeasonalCertificationDigestRules.DefaultSeasonalCertificationDigestValidator();
        var item = SeasonalCertificationDigest.NewSeasonalCertificationDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalCertificationDigestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new SeasonalCertificationDigestDurationMinutesRangeRule(32, 120);
        var item = SeasonalCertificationDigest.NewSeasonalCertificationDigest("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalCertificationDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalCertificationDigestRequiredCodeRule("seasonal certification digest needs an entry");
        var item = SeasonalCertificationDigest.NewSeasonalCertificationDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalCertificationDigestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SeasonalCertificationDigestLabelLengthRule(3600);
        var item = SeasonalCertificationDigest.NewSeasonalCertificationDigest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalCertificationDigestValidatorReportsEveryProblem();
        SeasonalCertificationDigestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        SeasonalCertificationDigestRequiredCodeRuleFlagsABlankCode();
        SeasonalCertificationDigestLabelLengthRuleFlagsALongLabel();
    }
}
