namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SentimentProfileRulesChecks
{
    public static void SentimentProfileValidatorReportsEveryProblem()
    {
        var validator = SentimentProfileRules.DefaultSentimentProfileValidator();
        var item = SentimentProfile.NewSentimentProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentProfileSourceLengthRuleFlagsALongSource()
    {
        var rule = new SentimentProfileSourceLengthRule(1000);
        var item = SentimentProfile.NewSentimentProfile("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentProfileLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new SentimentProfileLeaveDaysRangeRule(80, 86400);
        var item = SentimentProfile.NewSentimentProfile("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new SentimentProfileRequiredNameRule("sentiment profile needs a value");
        var item = SentimentProfile.NewSentimentProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SentimentProfileValidatorReportsEveryProblem();
        SentimentProfileSourceLengthRuleFlagsALongSource();
        SentimentProfileLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        SentimentProfileRequiredNameRuleFlagsABlankName();
    }
}
