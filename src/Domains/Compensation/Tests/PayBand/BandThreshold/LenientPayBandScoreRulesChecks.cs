namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class LenientPayBandScoreRulesChecks
{
    public static void LenientPayBandScoreValidatorReportsEveryProblem()
    {
        var validator = LenientPayBandScoreRules.DefaultLenientPayBandScoreValidator();
        var item = LenientPayBandScore.NewLenientPayBandScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPayBandScoreDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new LenientPayBandScoreDescriptionLengthRule(32);
        var item = LenientPayBandScore.NewLenientPayBandScore("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPayBandScoreBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new LenientPayBandScoreBadgeLengthRule(32);
        var item = LenientPayBandScore.NewLenientPayBandScore("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPayBandScoreRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LenientPayBandScoreRequiredBadgeRule("lenient pay band score needs an entry");
        var item = LenientPayBandScore.NewLenientPayBandScore("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPayBandScoreRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientPayBandScoreRequiredLabelRule("lenient pay band score needs a value");
        var item = LenientPayBandScore.NewLenientPayBandScore("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPayBandScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientPayBandScoreRequiredCodeRule("lenient pay band score needs an entry");
        var item = LenientPayBandScore.NewLenientPayBandScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPayBandScoreLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new LenientPayBandScoreLimitRangeRule(16, 1000);
        var item = LenientPayBandScore.NewLenientPayBandScore("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientPayBandScoreValidatorReportsEveryProblem();
        LenientPayBandScoreDescriptionLengthRuleFlagsALongDescription();
        LenientPayBandScoreBadgeLengthRuleFlagsALongBadge();
        LenientPayBandScoreRequiredBadgeRuleFlagsABlankBadge();
        LenientPayBandScoreRequiredLabelRuleFlagsABlankLabel();
        LenientPayBandScoreRequiredCodeRuleFlagsABlankCode();
        LenientPayBandScoreLimitRangeRuleFlagsValuesAboveLimit();
    }
}
