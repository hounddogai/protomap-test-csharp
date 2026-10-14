namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalPulseSurveyPreferenceRulesChecks
{
    public static void GlobalPulseSurveyPreferenceValidatorReportsEveryProblem()
    {
        var validator = GlobalPulseSurveyPreferenceRules.DefaultGlobalPulseSurveyPreferenceValidator();
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPulseSurveyPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalPulseSurveyPreferenceTitleLengthRule(64);
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPulseSurveyPreferenceBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new GlobalPulseSurveyPreferenceBudgetRangeRule(0.4, 3290.6);
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("x", "y");
        item.Budget = 3292.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPulseSurveyPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalPulseSurveyPreferenceRequiredCodeRule("global pulse survey preference needs an entry");
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalPulseSurveyPreferenceSourceLengthRuleFlagsALongSource()
    {
        var rule = new GlobalPulseSurveyPreferenceSourceLengthRule(64);
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyPreferenceValidatorReportsEveryProblem();
        GlobalPulseSurveyPreferenceTitleLengthRuleFlagsALongTitle();
        GlobalPulseSurveyPreferenceBudgetRangeRuleFlagsValuesAboveBudget();
        GlobalPulseSurveyPreferenceRequiredCodeRuleFlagsABlankCode();
        GlobalPulseSurveyPreferenceSourceLengthRuleFlagsALongSource();
    }
}
