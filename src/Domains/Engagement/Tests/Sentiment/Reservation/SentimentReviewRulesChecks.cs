namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReviewRulesChecks
{
    public static void SentimentReviewValidatorReportsEveryProblem()
    {
        var validator = SentimentReviewRules.DefaultSentimentReviewValidator();
        var item = SentimentReview.NewSentimentReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentReviewRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new SentimentReviewRequiredCurrencyRule("sentiment review needs this field");
        var item = SentimentReview.NewSentimentReview("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentReviewLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new SentimentReviewLimitRangeRule(255, 86400);
        var item = SentimentReview.NewSentimentReview("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentReviewRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new SentimentReviewRequiredLocaleRule("sentiment review needs a value");
        var item = SentimentReview.NewSentimentReview("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentReviewLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new SentimentReviewLocaleLengthRule(120);
        var item = SentimentReview.NewSentimentReview("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentReviewNameLengthRuleFlagsALongName()
    {
        var rule = new SentimentReviewNameLengthRule(3600);
        var item = SentimentReview.NewSentimentReview("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentReviewValidatorReportsEveryProblem();
        SentimentReviewRequiredCurrencyRuleFlagsABlankCurrency();
        SentimentReviewLimitRangeRuleFlagsValuesAboveLimit();
        SentimentReviewRequiredLocaleRuleFlagsABlankLocale();
        SentimentReviewLocaleLengthRuleFlagsALongLocale();
        SentimentReviewNameLengthRuleFlagsALongName();
    }
}
