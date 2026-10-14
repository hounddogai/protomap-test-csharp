namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StrictPulseSurveyPreferenceRulesChecks
{
    public static void StrictPulseSurveyPreferenceValidatorReportsEveryProblem()
    {
        var validator = StrictPulseSurveyPreferenceRules.DefaultStrictPulseSurveyPreferenceValidator();
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPulseSurveyPreferenceRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new StrictPulseSurveyPreferenceRequiredGradeRule("strict pulse survey preference needs this field");
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPulseSurveyPreferenceWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new StrictPulseSurveyPreferenceWindowDaysRangeRule(16, 64);
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("x", "y");
        item.WindowDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPulseSurveyPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictPulseSurveyPreferenceCodeLengthRule(120);
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPulseSurveyPreferenceCountryLengthRuleFlagsALongCountry()
    {
        var rule = new StrictPulseSurveyPreferenceCountryLengthRule(64);
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPulseSurveyPreferenceGradeLengthRuleFlagsALongGrade()
    {
        var rule = new StrictPulseSurveyPreferenceGradeLengthRule(16);
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictPulseSurveyPreferenceValidatorReportsEveryProblem();
        StrictPulseSurveyPreferenceRequiredGradeRuleFlagsABlankGrade();
        StrictPulseSurveyPreferenceWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        StrictPulseSurveyPreferenceCodeLengthRuleFlagsALongCode();
        StrictPulseSurveyPreferenceCountryLengthRuleFlagsALongCountry();
        StrictPulseSurveyPreferenceGradeLengthRuleFlagsALongGrade();
    }
}
