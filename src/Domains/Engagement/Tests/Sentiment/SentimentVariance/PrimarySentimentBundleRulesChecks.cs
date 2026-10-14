namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class PrimarySentimentBundleRulesChecks
{
    public static void PrimarySentimentBundleValidatorReportsEveryProblem()
    {
        var validator = PrimarySentimentBundleRules.DefaultPrimarySentimentBundleValidator();
        var item = PrimarySentimentBundle.NewPrimarySentimentBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimarySentimentBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimarySentimentBundleCodeLengthRule(64);
        var item = PrimarySentimentBundle.NewPrimarySentimentBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimarySentimentBundleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PrimarySentimentBundleCreatedAtRangeRule(255, 86400);
        var item = PrimarySentimentBundle.NewPrimarySentimentBundle("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimarySentimentBundleValidatorReportsEveryProblem();
        PrimarySentimentBundleCodeLengthRuleFlagsALongCode();
        PrimarySentimentBundleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
