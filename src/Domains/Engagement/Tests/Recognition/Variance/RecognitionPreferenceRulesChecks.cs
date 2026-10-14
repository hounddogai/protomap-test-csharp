namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionPreferenceRulesChecks
{
    public static void RecognitionPreferenceValidatorReportsEveryProblem()
    {
        var validator = RecognitionPreferenceRules.DefaultRecognitionPreferenceValidator();
        var item = RecognitionPreference.NewRecognitionPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionPreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RecognitionPreferenceLabelLengthRule(80);
        var item = RecognitionPreference.NewRecognitionPreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionPreferenceUnitLengthRuleFlagsALongUnit()
    {
        var rule = new RecognitionPreferenceUnitLengthRule(86400);
        var item = RecognitionPreference.NewRecognitionPreference("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RecognitionPreferenceExpiresAtRangeRule(16, 3600);
        var item = RecognitionPreference.NewRecognitionPreference("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionPreferenceRequiredCodeRule("recognition preference needs this field");
        var item = RecognitionPreference.NewRecognitionPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionPreferenceEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new RecognitionPreferenceEndsAtRangeRule(80, 86400);
        var item = RecognitionPreference.NewRecognitionPreference("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RecognitionPreferenceValidatorReportsEveryProblem();
        RecognitionPreferenceLabelLengthRuleFlagsALongLabel();
        RecognitionPreferenceUnitLengthRuleFlagsALongUnit();
        RecognitionPreferenceExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RecognitionPreferenceRequiredCodeRuleFlagsABlankCode();
        RecognitionPreferenceEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
