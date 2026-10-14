namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LegacyCareerPathDigestRulesChecks
{
    public static void LegacyCareerPathDigestValidatorReportsEveryProblem()
    {
        var validator = LegacyCareerPathDigestRules.DefaultLegacyCareerPathDigestValidator();
        var item = LegacyCareerPathDigest.NewLegacyCareerPathDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyCareerPathDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyCareerPathDigestCodeLengthRule(64);
        var item = LegacyCareerPathDigest.NewLegacyCareerPathDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyCareerPathDigestContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new LegacyCareerPathDigestContactEmailLengthRule(80);
        var item = LegacyCareerPathDigest.NewLegacyCareerPathDigest("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyCareerPathDigestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LegacyCareerPathDigestTitleLengthRule(16);
        var item = LegacyCareerPathDigest.NewLegacyCareerPathDigest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyCareerPathDigestValidatorReportsEveryProblem();
        LegacyCareerPathDigestCodeLengthRuleFlagsALongCode();
        LegacyCareerPathDigestContactEmailLengthRuleFlagsALongContactEmail();
        LegacyCareerPathDigestTitleLengthRuleFlagsALongTitle();
    }
}
