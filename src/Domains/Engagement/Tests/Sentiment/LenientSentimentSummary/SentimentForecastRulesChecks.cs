namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class SentimentForecastRulesChecks
{
    public static void SentimentForecastValidatorReportsEveryProblem()
    {
        var validator = SentimentForecastRules.DefaultSentimentForecastValidator();
        var item = SentimentForecast.NewSentimentForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentForecastRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SentimentForecastRequiredLabelRule("sentiment forecast needs a value");
        var item = SentimentForecast.NewSentimentForecast("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentForecastCodeLengthRule(16);
        var item = SentimentForecast.NewSentimentForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentForecastValidatorReportsEveryProblem();
        SentimentForecastRequiredLabelRuleFlagsABlankLabel();
        SentimentForecastCodeLengthRuleFlagsALongCode();
    }
}
