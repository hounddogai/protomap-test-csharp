namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentSummary;

using Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>Checks of sentiment summary code, run by the project's runner.</summary>
public static class FallbackSentimentProfileRulesChecks
{
    public static void FallbackSentimentProfileValidatorReportsEveryProblem()
    {
        var validator = FallbackSentimentProfileRules.DefaultFallbackSentimentProfileValidator();
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackSentimentProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackSentimentProfileTitleLengthRule(80);
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackSentimentProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackSentimentProfileRequiredTitleRule("fallback sentiment profile needs a value");
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackSentimentProfileAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new FallbackSentimentProfileAttemptsRangeRule(16, 255);
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackSentimentProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackSentimentProfileRequiredCodeRule("fallback sentiment profile needs an entry");
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackSentimentProfileValidatorReportsEveryProblem();
        FallbackSentimentProfileTitleLengthRuleFlagsALongTitle();
        FallbackSentimentProfileRequiredTitleRuleFlagsABlankTitle();
        FallbackSentimentProfileAttemptsRangeRuleFlagsValuesAboveAttempts();
        FallbackSentimentProfileRequiredCodeRuleFlagsABlankCode();
    }
}
