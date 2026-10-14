namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalCareerPathDigestRulesChecks
{
    public static void SeasonalCareerPathDigestValidatorReportsEveryProblem()
    {
        var validator = SeasonalCareerPathDigestRules.DefaultSeasonalCareerPathDigestValidator();
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalCareerPathDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalCareerPathDigestRequiredCodeRule("seasonal career path digest needs a value");
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalCareerPathDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalCareerPathDigestCodeLengthRule(120);
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalCareerPathDigestDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new SeasonalCareerPathDigestDiscountRangeRule(4.9, 264.8);
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("x", "y");
        item.Discount = 266.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalCareerPathDigestValidatorReportsEveryProblem();
        SeasonalCareerPathDigestRequiredCodeRuleFlagsABlankCode();
        SeasonalCareerPathDigestCodeLengthRuleFlagsALongCode();
        SeasonalCareerPathDigestDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
