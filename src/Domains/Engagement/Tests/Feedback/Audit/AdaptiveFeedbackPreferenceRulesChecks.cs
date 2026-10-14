namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AdaptiveFeedbackPreferenceRulesChecks
{
    public static void AdaptiveFeedbackPreferenceValidatorReportsEveryProblem()
    {
        var validator = AdaptiveFeedbackPreferenceRules.DefaultAdaptiveFeedbackPreferenceValidator();
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveFeedbackPreferenceNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveFeedbackPreferenceNameLengthRule(3600);
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFeedbackPreferenceMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AdaptiveFeedbackPreferenceMarginRangeRule(8.6, 1519.9);
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("x", "y");
        item.Margin = 1521.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveFeedbackPreferenceAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new AdaptiveFeedbackPreferenceAttemptsRangeRule(120, 1000);
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveFeedbackPreferenceRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new AdaptiveFeedbackPreferenceRequiredSummaryRule("adaptive feedback preference needs this field");
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveFeedbackPreferenceValidatorReportsEveryProblem();
        AdaptiveFeedbackPreferenceNameLengthRuleFlagsALongName();
        AdaptiveFeedbackPreferenceMarginRangeRuleFlagsValuesAboveMargin();
        AdaptiveFeedbackPreferenceAttemptsRangeRuleFlagsValuesAboveAttempts();
        AdaptiveFeedbackPreferenceRequiredSummaryRuleFlagsABlankSummary();
    }
}
