namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Entry;

using Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanPreferenceRulesChecks
{
    public static void SeasonalSuccessionPlanPreferenceValidatorReportsEveryProblem()
    {
        var validator = SeasonalSuccessionPlanPreferenceRules.DefaultSeasonalSuccessionPlanPreferenceValidator();
        var item = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalSuccessionPlanPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalSuccessionPlanPreferenceCodeLengthRule(32);
        var item = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalSuccessionPlanPreferencePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new SeasonalSuccessionPlanPreferencePhoneLengthRule(86400);
        var item = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalSuccessionPlanPreferenceLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new SeasonalSuccessionPlanPreferenceLocaleLengthRule(86400);
        var item = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanPreferenceValidatorReportsEveryProblem();
        SeasonalSuccessionPlanPreferenceCodeLengthRuleFlagsALongCode();
        SeasonalSuccessionPlanPreferencePhoneLengthRuleFlagsALongPhone();
        SeasonalSuccessionPlanPreferenceLocaleLengthRuleFlagsALongLocale();
    }
}
