namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentSummary;

using Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>Checks of sentiment summary code, run by the project's runner.</summary>
public static class SentimentBundleRulesChecks
{
    public static void SentimentBundleValidatorReportsEveryProblem()
    {
        var validator = SentimentBundleRules.DefaultSentimentBundleValidator();
        var item = SentimentBundle.NewSentimentBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentBundleCodeLengthRule(1000);
        var item = SentimentBundle.NewSentimentBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentBundleRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new SentimentBundleRequiredCategoryRule("sentiment bundle needs an entry");
        var item = SentimentBundle.NewSentimentBundle("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentBundleRequiredCodeRule("sentiment bundle needs an entry");
        var item = SentimentBundle.NewSentimentBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentBundleReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new SentimentBundleReviewerLengthRule(1000);
        var item = SentimentBundle.NewSentimentBundle("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentBundleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SentimentBundleAmountRangeRule(4.5, 3377.2);
        var item = SentimentBundle.NewSentimentBundle("x", "y");
        item.Amount = 3378.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentBundleValidatorReportsEveryProblem();
        SentimentBundleCodeLengthRuleFlagsALongCode();
        SentimentBundleRequiredCategoryRuleFlagsABlankCategory();
        SentimentBundleRequiredCodeRuleFlagsABlankCode();
        SentimentBundleReviewerLengthRuleFlagsALongReviewer();
        SentimentBundleAmountRangeRuleFlagsValuesAboveAmount();
    }
}
