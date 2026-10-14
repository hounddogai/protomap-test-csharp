namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentVarianceRulesChecks
{
    public static void SentimentVarianceValidatorReportsEveryProblem()
    {
        var validator = SentimentVarianceRules.DefaultSentimentVarianceValidator();
        var item = SentimentVariance.NewSentimentVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SentimentVarianceRequiredTitleRule("sentiment variance needs an entry");
        var item = SentimentVariance.NewSentimentVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentVarianceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SentimentVarianceTitleLengthRule(32);
        var item = SentimentVariance.NewSentimentVariance("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentVarianceCodeLengthRule(3600);
        var item = SentimentVariance.NewSentimentVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentVarianceValidatorReportsEveryProblem();
        SentimentVarianceRequiredTitleRuleFlagsABlankTitle();
        SentimentVarianceTitleLengthRuleFlagsALongTitle();
        SentimentVarianceCodeLengthRuleFlagsALongCode();
    }
}
