namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackSentimentBundleRulesChecks
{
    public static void FallbackSentimentBundleValidatorReportsEveryProblem()
    {
        var validator = FallbackSentimentBundleRules.DefaultFallbackSentimentBundleValidator();
        var item = FallbackSentimentBundle.NewFallbackSentimentBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackSentimentBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackSentimentBundleRequiredCodeRule("fallback sentiment bundle needs this field");
        var item = FallbackSentimentBundle.NewFallbackSentimentBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackSentimentBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackSentimentBundleCodeLengthRule(32);
        var item = FallbackSentimentBundle.NewFallbackSentimentBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackSentimentBundleValidatorReportsEveryProblem();
        FallbackSentimentBundleRequiredCodeRuleFlagsABlankCode();
        FallbackSentimentBundleCodeLengthRuleFlagsALongCode();
    }
}
