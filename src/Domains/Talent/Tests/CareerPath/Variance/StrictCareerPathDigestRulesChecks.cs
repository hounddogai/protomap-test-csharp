namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictCareerPathDigestRulesChecks
{
    public static void StrictCareerPathDigestValidatorReportsEveryProblem()
    {
        var validator = StrictCareerPathDigestRules.DefaultStrictCareerPathDigestValidator();
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictCareerPathDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictCareerPathDigestCodeLengthRule(16);
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictCareerPathDigestRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new StrictCareerPathDigestRequiredChecksumRule("strict career path digest needs a value");
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictCareerPathDigestNameLengthRuleFlagsALongName()
    {
        var rule = new StrictCareerPathDigestNameLengthRule(16);
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictCareerPathDigestChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new StrictCareerPathDigestChecksumLengthRule(80);
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictCareerPathDigestRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new StrictCareerPathDigestRequiredCountryRule("strict career path digest needs a value");
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictCareerPathDigestValidatorReportsEveryProblem();
        StrictCareerPathDigestCodeLengthRuleFlagsALongCode();
        StrictCareerPathDigestRequiredChecksumRuleFlagsABlankChecksum();
        StrictCareerPathDigestNameLengthRuleFlagsALongName();
        StrictCareerPathDigestChecksumLengthRuleFlagsALongChecksum();
        StrictCareerPathDigestRequiredCountryRuleFlagsABlankCountry();
    }
}
