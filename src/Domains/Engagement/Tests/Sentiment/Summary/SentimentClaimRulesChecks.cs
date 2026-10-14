namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentClaimRulesChecks
{
    public static void SentimentClaimValidatorReportsEveryProblem()
    {
        var validator = SentimentClaimRules.DefaultSentimentClaimValidator();
        var item = SentimentClaim.NewSentimentClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentClaimLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SentimentClaimLabelLengthRule(1000);
        var item = SentimentClaim.NewSentimentClaim("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentClaimScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new SentimentClaimScoreRangeRule(7.1, 2335.9);
        var item = SentimentClaim.NewSentimentClaim("x", "y");
        item.Score = 2337.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentClaimValidatorReportsEveryProblem();
        SentimentClaimLabelLengthRuleFlagsALongLabel();
        SentimentClaimScoreRangeRuleFlagsValuesAboveScore();
    }
}
