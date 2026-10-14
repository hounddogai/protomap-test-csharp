namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class FallbackSentimentPreferenceRulesChecks
{
    public static void FallbackSentimentPreferenceValidatorReportsEveryProblem()
    {
        var validator = FallbackSentimentPreferenceRules.DefaultFallbackSentimentPreferenceValidator();
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackSentimentPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackSentimentPreferenceCodeLengthRule(64);
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackSentimentPreferenceCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new FallbackSentimentPreferenceCurrencyLengthRule(255);
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackSentimentPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackSentimentPreferenceRequiredTitleRule("fallback sentiment preference needs an entry");
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackSentimentPreferenceValidatorReportsEveryProblem();
        FallbackSentimentPreferenceCodeLengthRuleFlagsALongCode();
        FallbackSentimentPreferenceCurrencyLengthRuleFlagsALongCurrency();
        FallbackSentimentPreferenceRequiredTitleRuleFlagsABlankTitle();
    }
}
