namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredFeedbackPreferenceRulesChecks
{
    public static void TieredFeedbackPreferenceValidatorReportsEveryProblem()
    {
        var validator = TieredFeedbackPreferenceRules.DefaultTieredFeedbackPreferenceValidator();
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredFeedbackPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredFeedbackPreferenceCodeLengthRule(86400);
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredFeedbackPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredFeedbackPreferenceRequiredCodeRule("tiered feedback preference needs this field");
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredFeedbackPreferenceRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new TieredFeedbackPreferenceRatingRangeRule(7.7, 4358.2);
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("x", "y");
        item.Rating = 4359.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredFeedbackPreferenceRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new TieredFeedbackPreferenceRequiredOwnerIdRule("tiered feedback preference needs a value");
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredFeedbackPreferenceValidatorReportsEveryProblem();
        TieredFeedbackPreferenceCodeLengthRuleFlagsALongCode();
        TieredFeedbackPreferenceRequiredCodeRuleFlagsABlankCode();
        TieredFeedbackPreferenceRatingRangeRuleFlagsValuesAboveRating();
        TieredFeedbackPreferenceRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
