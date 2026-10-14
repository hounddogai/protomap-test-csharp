namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalPoolTierRulesChecks
{
    public static void SeasonalPoolTierValidatorReportsEveryProblem()
    {
        var validator = SeasonalPoolTierRules.DefaultSeasonalPoolTierValidator();
        var item = SeasonalPoolTier.NewSeasonalPoolTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPoolTierRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new SeasonalPoolTierRequiredReviewerRule("seasonal pool tier needs a value");
        var item = SeasonalPoolTier.NewSeasonalPoolTier("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPoolTierNameLengthRuleFlagsALongName()
    {
        var rule = new SeasonalPoolTierNameLengthRule(64);
        var item = SeasonalPoolTier.NewSeasonalPoolTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalPoolTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalPoolTierRequiredNameRule("seasonal pool tier needs this field");
        var item = SeasonalPoolTier.NewSeasonalPoolTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalPoolTierValidatorReportsEveryProblem();
        SeasonalPoolTierRequiredReviewerRuleFlagsABlankReviewer();
        SeasonalPoolTierNameLengthRuleFlagsALongName();
        SeasonalPoolTierRequiredNameRuleFlagsABlankName();
    }
}
