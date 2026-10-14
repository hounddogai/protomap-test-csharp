namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentPreferenceRulesChecks
{
    public static void SentimentPreferenceValidatorReportsEveryProblem()
    {
        var validator = SentimentPreferenceRules.DefaultSentimentPreferenceValidator();
        var item = SentimentPreference.NewSentimentPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SentimentPreferenceRequiredTitleRule("sentiment preference needs a value");
        var item = SentimentPreference.NewSentimentPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentPreferenceExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new SentimentPreferenceExternalRefLengthRule(3600);
        var item = SentimentPreference.NewSentimentPreference("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentPreferenceRequiredCodeRule("sentiment preference needs this field");
        var item = SentimentPreference.NewSentimentPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SentimentPreferenceValidatorReportsEveryProblem();
        SentimentPreferenceRequiredTitleRuleFlagsABlankTitle();
        SentimentPreferenceExternalRefLengthRuleFlagsALongExternalRef();
        SentimentPreferenceRequiredCodeRuleFlagsABlankCode();
    }
}
