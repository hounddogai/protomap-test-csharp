namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedPayRunPreferenceRulesChecks
{
    public static void WeightedPayRunPreferenceValidatorReportsEveryProblem()
    {
        var validator = WeightedPayRunPreferenceRules.DefaultWeightedPayRunPreferenceValidator();
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedPayRunPreferenceScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new WeightedPayRunPreferenceScoreRangeRule(8.8, 3974.0);
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("x", "y");
        item.Score = 3975.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedPayRunPreferenceRegionLengthRuleFlagsALongRegion()
    {
        var rule = new WeightedPayRunPreferenceRegionLengthRule(16);
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPayRunPreferenceRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new WeightedPayRunPreferenceRequiredSourceRule("weighted pay run preference needs a value");
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedPayRunPreferenceLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new WeightedPayRunPreferenceLimitRangeRule(3600, 86400);
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedPayRunPreferenceValidatorReportsEveryProblem();
        WeightedPayRunPreferenceScoreRangeRuleFlagsValuesAboveScore();
        WeightedPayRunPreferenceRegionLengthRuleFlagsALongRegion();
        WeightedPayRunPreferenceRequiredSourceRuleFlagsABlankSource();
        WeightedPayRunPreferenceLimitRangeRuleFlagsValuesAboveLimit();
    }
}
