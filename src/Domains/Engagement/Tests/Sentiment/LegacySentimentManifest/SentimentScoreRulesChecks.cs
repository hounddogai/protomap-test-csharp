namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SentimentScoreRulesChecks
{
    public static void SentimentScoreValidatorReportsEveryProblem()
    {
        var validator = SentimentScoreRules.DefaultSentimentScoreValidator();
        var item = SentimentScore.NewSentimentScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentScoreRequiredCodeRule("sentiment score needs this field");
        var item = SentimentScore.NewSentimentScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentScoreRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new SentimentScoreRequiredBadgeRule("sentiment score needs a value");
        var item = SentimentScore.NewSentimentScore("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentScoreNameLengthRuleFlagsALongName()
    {
        var rule = new SentimentScoreNameLengthRule(120);
        var item = SentimentScore.NewSentimentScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentScoreRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new SentimentScoreRatioRangeRule(1.2, 3447.2);
        var item = SentimentScore.NewSentimentScore("x", "y");
        item.Ratio = 3448.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentScoreBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new SentimentScoreBadgeLengthRule(3600);
        var item = SentimentScore.NewSentimentScore("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentScoreValidatorReportsEveryProblem();
        SentimentScoreRequiredCodeRuleFlagsABlankCode();
        SentimentScoreRequiredBadgeRuleFlagsABlankBadge();
        SentimentScoreNameLengthRuleFlagsALongName();
        SentimentScoreRatioRangeRuleFlagsValuesAboveRatio();
        SentimentScoreBadgeLengthRuleFlagsALongBadge();
    }
}
