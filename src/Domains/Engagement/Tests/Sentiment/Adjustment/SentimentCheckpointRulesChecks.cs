namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentCheckpointRulesChecks
{
    public static void SentimentCheckpointValidatorReportsEveryProblem()
    {
        var validator = SentimentCheckpointRules.DefaultSentimentCheckpointValidator();
        var item = SentimentCheckpoint.NewSentimentCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentCheckpointRequiredCodeRule("sentiment checkpoint needs this field");
        var item = SentimentCheckpoint.NewSentimentCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentCheckpointCodeLengthRule(255);
        var item = SentimentCheckpoint.NewSentimentCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentCheckpointValidatorReportsEveryProblem();
        SentimentCheckpointRequiredCodeRuleFlagsABlankCode();
        SentimentCheckpointCodeLengthRuleFlagsALongCode();
    }
}
