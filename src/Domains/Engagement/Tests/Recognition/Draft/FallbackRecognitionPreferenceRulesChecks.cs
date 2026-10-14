namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackRecognitionPreferenceRulesChecks
{
    public static void FallbackRecognitionPreferenceValidatorReportsEveryProblem()
    {
        var validator = FallbackRecognitionPreferenceRules.DefaultFallbackRecognitionPreferenceValidator();
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackRecognitionPreferenceRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FallbackRecognitionPreferenceRequiredCountryRule(
            "fallback recognition preference needs this field");
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRecognitionPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackRecognitionPreferenceCodeLengthRule(64);
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackRecognitionPreferenceWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new FallbackRecognitionPreferenceWindowDaysRangeRule(80, 255);
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("x", "y");
        item.WindowDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackRecognitionPreferenceNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackRecognitionPreferenceNameLengthRule(255);
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackRecognitionPreferenceValidatorReportsEveryProblem();
        FallbackRecognitionPreferenceRequiredCountryRuleFlagsABlankCountry();
        FallbackRecognitionPreferenceCodeLengthRuleFlagsALongCode();
        FallbackRecognitionPreferenceWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        FallbackRecognitionPreferenceNameLengthRuleFlagsALongName();
    }
}
